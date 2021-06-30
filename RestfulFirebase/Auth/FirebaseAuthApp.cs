using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Extensions.Http;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ObservableHelpers;

namespace RestfulFirebase.Auth
{
    /// <summary>
    /// App module that provides firebase authentication implementations.
    /// </summary>
    public class FirebaseAuthApp : SyncContext
    {
        #region Properties

        /// <summary>
        /// Gets the underlying <see cref="RestfulFirebaseApp"/> the module uses.
        /// </summary>
        public RestfulFirebaseApp App { get; private set; }

        /// <summary>
        /// Gets the <see cref="RestfulFirebase.Auth.Session"/> of the authenticated token. Returns <c>null</c> if no authenticated token is present.
        /// </summary>
        public Session Session => session.Exist ? session : null;

        /// <summary>
        /// Gets <c>true</c> whether the user is authenticated; otherwise <c>false</c>.
        /// </summary>
        public bool IsAuthenticated => Session != null;

        /// <summary>
        /// Event raised on the current context when the authentication is refreshed.
        /// </summary>
        public event Action AuthRefreshed;

        /// <summary>
        /// Event raised on the current context when the module is authenticated.
        /// </summary>
        public event Action Authenticated;

        private IHttpClientProxy client;
        private Session session;

        internal const string GoogleRefreshAuth = "https://securetoken.googleapis.com/v1/token?key={0}";
        internal const string GoogleCustomAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key={0}";
        internal const string GoogleGetUser = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={0}";
        internal const string GoogleIdentityUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyAssertion?key={0}";
        internal const string GoogleSignUpUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}";
        internal const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
        internal const string GoogleDeleteUserUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/deleteAccount?key={0}";
        internal const string GoogleGetConfirmationCodeUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key={0}";
        internal const string GoogleSetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key={0}";
        internal const string GoogleCreateAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/createAuthUri?key={0}";
        internal const string GoogleUpdateUserPassword = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
        internal const string ProfileDeleteDisplayName = "DISPLAY_NAME";
        internal const string ProfileDeletePhotoUrl = "PHOTO_URL";

        #endregion

        #region Initializers

        internal FirebaseAuthApp(RestfulFirebaseApp app)
        {
            App = app;
            session = new Session(App);
        }

        #endregion

        #region Helpers

        internal static FirebaseExceptionReason GetFailureReason(string responseData)
        {
            var failureReason = FirebaseExceptionReason.AuthUndefined;
            try
            {
                if (!string.IsNullOrEmpty(responseData) && responseData != "N/A")
                {
                    //create error data template and try to parse JSON
                    var errorData = new { error = new { code = 0, message = "errorid" } };
                    errorData = JsonConvert.DeserializeAnonymousType(responseData, errorData);

                    //errorData is just null if different JSON was received
                    switch (errorData?.error?.message)
                    {
                        //general errors
                        case "invalid access_token, error code 43.":
                            failureReason = FirebaseExceptionReason.AuthInvalidAccessToken;
                            break;

                        case "CREDENTIAL_TOO_OLD_LOGIN_AGAIN":
                            failureReason = FirebaseExceptionReason.AuthLoginCredentialsTooOld;
                            break;

                        case "OPERATION_NOT_ALLOWED":
                            failureReason = FirebaseExceptionReason.AuthOperationNotAllowed;
                            break;

                        //possible errors from Third Party Authentication using GoogleIdentityUrl
                        case "INVALID_PROVIDER_ID : Provider Id is not supported.":
                            failureReason = FirebaseExceptionReason.AuthInvalidProviderID;
                            break;
                        case "MISSING_REQUEST_URI":
                            failureReason = FirebaseExceptionReason.AuthMissingRequestURI;
                            break;
                        case "A system error has occurred - missing or invalid postBody":
                            failureReason = FirebaseExceptionReason.AuthSystemError;
                            break;
                        case "MISSING_OR_INVALID_NONCE : Duplicate credential received. Please try again with a new credential.":
                            failureReason = FirebaseExceptionReason.AuthDuplicateCredentialUse;
                            break;

                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo) or Signin
                        case "INVALID_EMAIL":
                            failureReason = FirebaseExceptionReason.AuthInvalidEmailAddress;
                            break;
                        case "MISSING_PASSWORD":
                            failureReason = FirebaseExceptionReason.AuthMissingPassword;
                            break;

                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                        case "EMAIL_EXISTS":
                            failureReason = FirebaseExceptionReason.AuthEmailExists;
                            break;

                        //possible errors from Account Delete
                        case "USER_NOT_FOUND":
                            failureReason = FirebaseExceptionReason.AuthUserNotFound;
                            break;

                        //possible errors from Email/Password Signin
                        case "INVALID_PASSWORD":
                            failureReason = FirebaseExceptionReason.AuthWrongPassword;
                            break;
                        case "EMAIL_NOT_FOUND":
                            failureReason = FirebaseExceptionReason.AuthUnknownEmailAddress;
                            break;
                        case "USER_DISABLED":
                            failureReason = FirebaseExceptionReason.AuthUserDisabled;
                            break;

                        //possible errors from Email/Password Signin or Password Recovery or Email/Password Sign up using setAccountInfo
                        case "MISSING_EMAIL":
                            failureReason = FirebaseExceptionReason.AuthMissingEmail;
                            break;
                        case "RESET_PASSWORD_EXCEED_LIMIT":
                            failureReason = FirebaseExceptionReason.AuthResetPasswordExceedLimit;
                            break;

                        //possible errors from Password Recovery
                        case "MISSING_REQ_TYPE":
                            failureReason = FirebaseExceptionReason.AuthMissingRequestType;
                            break;

                        //possible errors from Account Linking
                        case "INVALID_ID_TOKEN":
                            failureReason = FirebaseExceptionReason.AuthInvalidIDToken;
                            break;

                        //possible errors from Getting Linked Accounts
                        case "INVALID_IDENTIFIER":
                            failureReason = FirebaseExceptionReason.AuthInvalidIdentifier;
                            break;
                        case "MISSING_IDENTIFIER":
                            failureReason = FirebaseExceptionReason.AuthMissingIdentifier;
                            break;
                        case "FEDERATED_USER_ID_ALREADY_LINKED":
                            failureReason = FirebaseExceptionReason.AuthAlreadyLinked;
                            break;
                    }

                    if (failureReason == FirebaseExceptionReason.Undefined)
                    {
                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                        if (errorData?.error?.message?.StartsWith("WEAK_PASSWORD :") ?? false) failureReason = FirebaseExceptionReason.AuthWeakPassword;
                        //possible errors from Email/Password Signin
                        else if (errorData?.error?.message?.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER :") ?? false) failureReason = FirebaseExceptionReason.AuthTooManyAttemptsTryLater;
                        else if (errorData?.error?.message?.StartsWith("ERROR_INVALID_CREDENTIAL") ?? false) failureReason = FirebaseExceptionReason.AuthStaleIDToken;
                    }
                }
            }
            catch (JsonException)
            {
                //the response wasn't JSON - no data to be parsed
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unexpected error trying to parse the response: {e}");
            }

            return failureReason;
        }

        internal string GetProviderId(FirebaseAuthType authType)
        {
            switch (authType)
            {
                case FirebaseAuthType.Facebook:
                case FirebaseAuthType.Google:
                case FirebaseAuthType.Apple:
                case FirebaseAuthType.Github:
                case FirebaseAuthType.Twitter:
                    return authType.ToEnumString();
                case FirebaseAuthType.EmailAndPassword:
                    throw new InvalidOperationException("Email auth type cannot be used like this. Use methods specific to email & password authentication.");
                default:
                    throw new NotImplementedException("");
            }
        }

        internal HttpClient GetClient()
        {
            if (client == null)
            {
                client = App.Config.HttpClientFactory.GetHttpClient(App.Config.AuthRequestTimeout);
            }

            return client.GetHttpClient();
        }

        internal async Task<FirebaseAuth> ExecuteWithPostContent(string googleUrl, string postContent)
        {
            string responseData = "N/A";

            try
            {
                var response = await GetClient().PostAsync(
                    new Uri(string.Format(googleUrl, App.Config.ApiKey)),
                    new StringContent(postContent, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var user = JsonConvert.DeserializeObject<User>(responseData);
                var auth = JsonConvert.DeserializeObject<FirebaseAuth>(responseData);

                auth.User = user;

                return auth;
            }
            catch (OperationCanceledException ex)
            {
                throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
            }
            catch (Exception ex)
            {
                Type s = ex.GetType();
                FirebaseExceptionReason errorReason = GetFailureReason(responseData);
                throw new FirebaseException(errorReason, ex);
            }
        }

        internal async Task<CallResult> RefreshUserInfo(FirebaseAuth auth)
        {
            try
            {
                var content = $"{{\"idToken\":\"{auth.FirebaseToken}\"}}";
                var responseData = "N/A";
                try
                {
                    var response = await GetClient().PostAsync(
                        new Uri(string.Format(GoogleGetUser, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var resultJson = JObject.Parse(responseData);
                    var user = JsonConvert.DeserializeObject<User>(resultJson["users"].First().ToString());

                    auth.User = user;

                    session.UpdateAuth(auth);

                    OnAuthRefreshed();

                    return CallResult.Success(user);
                }
                catch (OperationCanceledException ex)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                }
                catch (Exception ex)
                {
                    FirebaseExceptionReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseException(errorReason, ex);
                }
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes <see cref="AuthRefreshed"/> event into the current context.
        /// </summary>
        internal void OnAuthRefreshed()
        {
            ContextPost(delegate
            {
                AuthRefreshed?.Invoke();
            });
        }

        /// <summary>
        /// Invokes <see cref="Authenticated"/> event into the current context.
        /// </summary>
        internal void OnAuthenticated()
        {
            ContextPost(delegate
            {
                Authenticated?.Invoke();
            });
        }

        /// <summary>
        /// Creates user with the provided <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email">
        /// The email of the user.
        /// </param>
        /// <param name="password">
        /// The password of the user.
        /// </param>
        /// <param name="displayName">
        /// The display name of the user.
        /// </param>
        /// <param name="sendVerificationEmail">
        /// <c>true</c> to send email verification after user creation; otherwise, <c>false</c>.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> CreateUserWithEmailAndPassword(string email, string password, string displayName = "", bool sendVerificationEmail = false)
        {
            try
            {
                var content = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleSignUpUrl, content).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    // set display name
                    content = $"{{\"displayName\":\"{displayName}\",\"idToken\":\"{auth.FirebaseToken}\",\"returnSecureToken\":true}}";

                    await ExecuteWithPostContent(GoogleSetAccountUrl, content).ConfigureAwait(false);

                    auth.User.DisplayName = displayName;
                }

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                if (sendVerificationEmail)
                {
                    var sendVerification = await Session.SendEmailVerification().ConfigureAwait(false);
                    if (!sendVerification.IsSuccess) return sendVerification;
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in with custom token provided by firebase.
        /// </summary>
        /// <param name="customToken">
        /// The token provided by firebase.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInWithCustomToken(string customToken)
        {
            try
            {
                string content = $"{{\"token\":\"{customToken}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleCustomAuthUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in with oauth provided with <paramref name="authType"/> and <paramref name="oauthToken"/>.
        /// </summary>
        /// <param name="authType">
        /// The <see cref="FirebaseAuthType"/> of the oauth used.
        /// </param>
        /// <param name="oauthToken">
        /// The token of the provided <paramref name="authType"/> type.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInWithOAuth(FirebaseAuthType authType, string oauthToken)
        {
            try
            {
                var providerId = GetProviderId(authType);

                string content;

                switch (authType)
                {
                    case FirebaseAuthType.Apple:
                        content = $"{{\"postBody\":\"id_token={oauthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";
                        break;
                    default:
                        content = $"{{\"postBody\":\"access_token={oauthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";
                        break;
                }

                var auth = await ExecuteWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in with twitter oauth token provided with <paramref name="oauthAccessToken"/> and <paramref name="oauthTokenSecret"/> from twitter.
        /// </summary>
        /// <param name="oauthAccessToken">
        /// The access token provided by twitter.
        /// </param>
        /// <param name="oauthTokenSecret">
        /// The oauth token secret provided by twitter
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInWithOAuthTwitterToken(string oauthAccessToken, string oauthTokenSecret)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Twitter);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&oauth_token_secret={oauthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in with google id token.
        /// </summary>
        /// <param name="idToken">
        /// The id token provided by google.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInWithGoogleIdToken(string idToken)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Google);
                var content = $"{{\"postBody\":\"id_token={idToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in with provided <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email">
        /// The email of the user.
        /// </param>
        /// <param name="password">
        /// The password of the user.
        /// </param>
        /// <param name="tenantId">
        /// The account tenant id of the user.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInWithEmailAndPassword(string email, string password, string tenantId = null)
        {
            try
            {
                StringBuilder sb = new StringBuilder($"{{\"email\":\"{email}\",\"password\":\"{password}\",");

                if (tenantId != null)
                {
                    sb.Append($"\"tenantId\":\"{tenantId}\",");
                }

                sb.Append("\"returnSecureToken\":true}");

                var auth = await ExecuteWithPostContent(GooglePasswordUrl, sb.ToString()).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign in anonimously.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SignInAnonymously()
        {
            try
            {
                var content = $"{{\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleSignUpUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                OnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Send password reset email to the existing account provided with the <paramref name="email"/>.
        /// </summary>
        /// <param name="email">
        /// The email of the user to send the password reset.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SendPasswordResetEmail(string email)
        {
            try
            {
                var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"}}";
                var responseData = "N/A";

                try
                {
                    var response = await GetClient().PostAsync(
                        new Uri(string.Format(GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                }
                catch (OperationCanceledException ex)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                }
                catch (Exception ex)
                {
                    FirebaseExceptionReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseException(errorReason, ex);
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
