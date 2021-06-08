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
    public class FirebaseAuthApp : SyncContext
    {
        #region Properties

        private const string GoogleRefreshAuth = "https://securetoken.googleapis.com/v1/token?key={0}";
        private const string GoogleCustomAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key={0}";
        private const string GoogleGetUser = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={0}";
        private const string GoogleIdentityUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyAssertion?key={0}";
        private const string GoogleSignUpUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}";
        private const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
        private const string GoogleDeleteUserUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/deleteAccount?key={0}";
        private const string GoogleGetConfirmationCodeUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key={0}";
        private const string GoogleSetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key={0}";
        private const string GoogleCreateAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/createAuthUri?key={0}";
        private const string GoogleUpdateUserPassword = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
        private const string ProfileDeleteDisplayName = "DISPLAY_NAME";
        private const string ProfileDeletePhotoUrl = "PHOTO_URL";

        private IHttpClientProxy client;
        private Session session;

        public RestfulFirebaseApp App { get; private set; }
        public Session Session => session.Exist ? session : null;
        public bool IsAuthenticated => Session != null;

        public event Action AuthRefreshed;
        public event Action Authenticated;

        #endregion

        #region Initializers

        internal FirebaseAuthApp(RestfulFirebaseApp app)
        {
            App = app;
            session = new Session(App);
        }

        #endregion

        #region Helpers

        private async Task<FirebaseAuth> ExecuteWithPostContent(string googleUrl, string postContent)
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

        private static FirebaseExceptionReason GetFailureReason(string responseData)
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

        private string GetProviderId(FirebaseAuthType authType)
        {
            switch (authType)
            {
                case FirebaseAuthType.Facebook:
                case FirebaseAuthType.Google:
                case FirebaseAuthType.Github:
                case FirebaseAuthType.Twitter:
                    return authType.ToEnumString();
                case FirebaseAuthType.EmailAndPassword:
                    throw new InvalidOperationException("Email auth type cannot be used like this. Use methods specific to email & password authentication.");
                default:
                    throw new NotImplementedException("");
            }
        }

        private HttpClient GetClient()
        {
            if (client == null)
            {
                client = App.Config.HttpClientFactory.GetHttpClient(App.Config.AuthRequestTimeout);
            }

            return client.GetHttpClient();
        }

        private async Task<CallResult> RefreshUserInfo(FirebaseAuth auth)
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

        protected void OnAuthRefreshed()
        {
            SynchronizationContextPost(delegate
            {
                AuthRefreshed?.Invoke();
            });
        }

        protected void OnAuthenticated()
        {
            SynchronizationContextPost(delegate
            {
                Authenticated?.Invoke();
            });
        }

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
                    var sendVerification = await SendEmailVerification().ConfigureAwait(false);
                    if (!sendVerification.IsSuccess) return sendVerification;
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

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

        public async Task<CallResult> SignInWithOAuth(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                var providerId = GetProviderId(authType);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

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

        public async Task<CallResult> ChangeUserPassword(string password)
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"idToken\":\"{token}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleUpdateUserPassword, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

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

        public async Task<CallResult> DeleteUser()
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{ \"idToken\": \"{token}\" }}";
                var responseData = "N/A";

                try
                {
                    var response = await GetClient().PostAsync(
                        new Uri(string.Format(GoogleDeleteUserUrl, App.Config.ApiKey)),
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

        public async Task<CallResult> SendEmailVerification()
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{token}\"}}";
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

        public async Task<CallResult> LinkAccounts(string email, string password)
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"idToken\":\"{token}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleSetAccountUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> LinkAccounts(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var providerId = GetProviderId(authType);
                var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContent(GoogleIdentityUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> UnlinkAccounts(FirebaseAuthType authType)
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                string providerId;
                if (authType == FirebaseAuthType.EmailAndPassword)
                {
                    providerId = authType.ToEnumString();
                }
                else
                {
                    providerId = GetProviderId(authType);
                }

                var content = $"{{\"idToken\":\"{token}\",\"deleteProvider\":[\"{providerId}\"]}}";

                var auth = await ExecuteWithPostContent(GoogleSetAccountUrl, content).ConfigureAwait(false);

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult<ProviderQueryResult>> GetLinkedAccounts()
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));
                var email = session.Email;
                if (string.IsNullOrEmpty(email)) throw new FirebaseException(FirebaseExceptionReason.AuthMissingEmail, new Exception("Email not found"));

                string content = $"{{\"identifier\":\"{email}\", \"continueUri\": \"http://localhost\"}}";
                string responseData = "N/A";

                ProviderQueryResult data;

                try
                {
                    var response = await GetClient().PostAsync(
                        new Uri(string.Format(GoogleCreateAuthUrl, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    data = JsonConvert.DeserializeObject<ProviderQueryResult>(responseData);
                    data.Email = email;
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

                return CallResult.Success<ProviderQueryResult>(data);
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error<ProviderQueryResult>(ex);
            }
        }

        public async Task<CallResult<string>> GetFreshToken()
        {
            try
            {
                var token = session.RefreshToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                if (session.IsExpired())
                {
                    var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{token}\"}}";
                    var responseData = "N/A";

                    try
                    {
                        HttpResponseMessage response = null;
                        response = await GetClient().PostAsync(
                            new Uri(string.Format(GoogleRefreshAuth, App.Config.ApiKey)),
                            new StringContent(content, Encoding.UTF8, "Application/json"),
                            new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);

                        responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var refreshAuth = JsonConvert.DeserializeObject<RefreshAuth>(responseData);

                        var auth = new FirebaseAuth
                        {
                            ExpiresIn = refreshAuth.ExpiresIn,
                            RefreshToken = refreshAuth.RefreshToken,
                            FirebaseToken = refreshAuth.AccessToken
                        };

                        session.UpdateAuth(auth);

                        OnAuthRefreshed();
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                    }
                    catch (Exception ex)
                    {
                        throw new FirebaseException(FirebaseExceptionReason.AuthUndefined, ex);
                    }
                }

                return CallResult.Success(session.FirebaseToken);
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error<string>(ex);
            }
        }

        public async Task<CallResult> RefreshAuth()
        {
            try
            {
                var session = Session;
                if (session == null) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var refresh = await GetFreshToken();
                if (!refresh.IsSuccess) return refresh;

                var auth = new FirebaseAuth()
                {
                    FirebaseToken = session.FirebaseToken,
                    RefreshToken = session.RefreshToken,
                    ExpiresIn = session.ExpiresIn,
                    Created = session.Created
                };

                var refreshResult = await RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> UpdateProfile(string displayName, string photoUrl)
        {
            try
            {
                var token = session.FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                StringBuilder sb = new StringBuilder($"{{\"idToken\":\"{token}\"");
                if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(photoUrl))
                {
                    sb.Append($",\"displayName\":\"{displayName}\",\"photoUrl\":\"{photoUrl}\"");
                }
                else if (!string.IsNullOrWhiteSpace(displayName))
                {
                    sb.Append($",\"displayName\":\"{displayName}\"");
                    sb.Append($",\"deleteAttribute\":[\"{ProfileDeletePhotoUrl}\"]");
                }
                else if (!string.IsNullOrWhiteSpace(photoUrl))
                {
                    sb.Append($",\"photoUrl\":\"{photoUrl}\"");
                    sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\"]");
                }
                else
                {
                    sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\",\"{ProfileDeletePhotoUrl}\"]");
                }

                sb.Append($",\"returnSecureToken\":true}}");

                var auth = await ExecuteWithPostContent(GoogleSetAccountUrl, sb.ToString()).ConfigureAwait(false);

                session.UpdateAuth(auth);

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> Signout()
        {
            try
            {
                if (!IsAuthenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                session.Purge();

                return await Task.FromResult(CallResult.Success());
            }
            catch (FirebaseException ex)
            {
                return await Task.FromResult(CallResult.Error(ex));
            }
        }

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
