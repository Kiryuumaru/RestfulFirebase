using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestfulFirebase.Common;
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

namespace RestfulFirebase.Auth
{
    public class FirebaseAuthApp : FirebaseAuth, IDisposable
    {
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
        private const string AuthRoot = "auth";

        private readonly HttpClient client;

        public RestfulFirebaseApp App { get; }

        public bool Authenticated
        {
            get
            {
                return 
                    User != null && 
                    !string.IsNullOrEmpty(FirebaseToken) && 
                    !string.IsNullOrEmpty(FirebaseToken);
            }
        }

        public event EventHandler<FirebaseAuthEventArgs> FirebaseAuthRefreshed;

        internal FirebaseAuthApp(RestfulFirebaseApp firebaseApp)
        {
            App = firebaseApp;
            client = new HttpClient();
            RetainPropertiesLocally();
        }

        public async Task<CallResult<FirebaseAuthException>> CreateUserWithEmailAndPasswordAsync(string email, string password, string displayName = "", bool sendVerificationEmail = false)
        {
            try
            {
                var content = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSignUpUrl, content).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    // set display name
                    content = $"{{\"displayName\":\"{displayName}\",\"idToken\":\"{auth.FirebaseToken}\",\"returnSecureToken\":true}}";

                    await ExecuteWithPostContentAsync(GoogleSetAccountUrl, content).ConfigureAwait(false);

                    auth.User.DisplayName = displayName;
                }

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                if (sendVerificationEmail)
                {
                    //send verification email
                    await SendEmailVerificationAsync().ConfigureAwait(false);
                }

                return CallResult<FirebaseAuthException>.Success();
            }
            catch(FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInWithCustomTokenAsync(string customToken)
        {
            try
            {
                string content = $"{{\"token\":\"{customToken}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleCustomAuthUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInWithOAuthAsync(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                var providerId = GetProviderId(authType);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInWithOAuthTwitterTokenAsync(string oauthAccessToken, string oauthTokenSecret)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Twitter);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&oauth_token_secret={oauthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInWithGoogleIdTokenAsync(string idToken)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Google);
                var content = $"{{\"postBody\":\"id_token={idToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInWithEmailAndPasswordAsync(string email, string password, string tenantId = null)
        {
            try
            {
                StringBuilder sb = new StringBuilder($"{{\"email\":\"{email}\",\"password\":\"{password}\",");

                if (tenantId != null)
                {
                    sb.Append($"\"tenantId\":\"{tenantId}\",");
                }

                sb.Append("\"returnSecureToken\":true}");

                var auth = await ExecuteWithPostContentAsync(GooglePasswordUrl, sb.ToString()).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SignInAnonymouslyAsync()
        {
            try
            {
                var content = $"{{\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSignUpUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> ChangeUserPassword(string password)
        {
            try
            {
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleUpdateUserPassword, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"}}";
                var responseData = "N/A";

                try
                {
                    var response = await client.PostAsync(
                        new Uri(string.Format(GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    AuthErrorReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseAuthException(GoogleGetConfirmationCodeUrl, content, responseData, ex, errorReason);
                }

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> DeleteUserAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                var content = $"{{ \"idToken\": \"{FirebaseToken}\" }}";
                var responseData = "N/A";

                try
                {
                    var response = await client.PostAsync(
                        new Uri(string.Format(GoogleDeleteUserUrl, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    AuthErrorReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseAuthException(GoogleDeleteUserUrl, content, responseData, ex, errorReason);
                }

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> SendEmailVerificationAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{FirebaseToken}\"}}";

                var response = await client.PostAsync(
                    new Uri(string.Format(GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                    new StringContent(content, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> LinkAccountsAsync(string email, string password)
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSetAccountUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> LinkAccountsAsync(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                var providerId = GetProviderId(authType);
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> UnlinkAccountsAsync(FirebaseAuthType authType)
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                string providerId;
                if (authType == FirebaseAuthType.EmailAndPassword)
                {
                    providerId = authType.ToEnumString();
                }
                else
                {
                    providerId = GetProviderId(authType);
                }

                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"deleteProvider\":[\"{providerId}\"]}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSetAccountUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<ProviderQueryResult, FirebaseAuthException>> GetLinkedAccountsAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                string content = $"{{\"identifier\":\"{User.Email}\", \"continueUri\": \"http://localhost\"}}";
                string responseData = "N/A";

                ProviderQueryResult data;

                try
                {
                    var response = await client.PostAsync(
                        new Uri(string.Format(GoogleCreateAuthUrl, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    data = JsonConvert.DeserializeObject<ProviderQueryResult>(responseData);
                    data.Email = User.Email;
                }
                catch (Exception ex)
                {
                    throw new FirebaseAuthException(GoogleCreateAuthUrl, content, responseData, ex);
                }

                return CallResult<ProviderQueryResult, FirebaseAuthException>.Success(data);
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<ProviderQueryResult, FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> RefreshUserDetailsAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                var content = $"{{\"idToken\":\"{FirebaseToken}\"}}";
                var responseData = "N/A";
                try
                {
                    var response = await client.PostAsync(
                        new Uri(string.Format(GoogleGetUser, App.Config.ApiKey)),
                        new StringContent(content, Encoding.UTF8, "Application/json"),
                        new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var resultJson = JObject.Parse(responseData);
                    var user = JsonConvert.DeserializeObject<User>(resultJson["users"].First().ToString());
                    User = user;
                }
                catch (Exception ex)
                {
                    AuthErrorReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseAuthException(GoogleDeleteUserUrl, content, responseData, ex, errorReason);
                }

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> RefreshAuthAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                if (IsExpired())
                {
                    var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{RefreshToken}\"}}";
                    var responseData = "N/A";

                    try
                    {
                        HttpResponseMessage response = null;
                        response = await client.PostAsync(
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

                        CopyPropertiesLocally(auth);
                        OnFirebaseAuthRefreshed(this);
                    }
                    catch (Exception ex)
                    {
                        throw new FirebaseAuthException(GoogleRefreshAuth, content, responseData, ex);
                    }
                }

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<string, FirebaseAuthException>> GetFreshTokenAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                if (IsExpired())
                {
                    var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{RefreshToken}\"}}";
                    var responseData = "N/A";

                    try
                    {
                        var response = await client.PostAsync(
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

                        CopyPropertiesLocally(auth);
                        OnFirebaseAuthRefreshed(this);
                    }
                    catch (Exception ex)
                    {
                        throw new FirebaseAuthException(GoogleRefreshAuth, content, responseData, ex);
                    }
                }

                return CallResult<string, FirebaseAuthException>.Success(FirebaseToken);
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<string, FirebaseAuthException>.Error(ex);
            }
        }

        public async Task<CallResult<FirebaseAuthException>> UpdateProfileAsync(string displayName, string photoUrl)
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);

                StringBuilder sb = new StringBuilder($"{{\"idToken\":\"{FirebaseToken}\"");
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

                var auth = await ExecuteWithPostContentAsync(GoogleSetAccountUrl, sb.ToString()).ConfigureAwait(false);
                CopyPropertiesLocally(auth);

                return CallResult<FirebaseAuthException>.Success();
            }
            catch (FirebaseAuthException ex)
            {
                return CallResult<FirebaseAuthException>.Error(ex);
            }
        }

        public Task<CallResult<FirebaseAuthException>> Signout()
        {
            try
            {
                if (!Authenticated) throw new FirebaseAuthException(new Exception("NOT AUTHENTICATED"), AuthErrorReason.NotAuthenticated);
                PurgePropertiesLocally();

                return Task.FromResult(CallResult<FirebaseAuthException>.Success());
            }
            catch (FirebaseAuthException ex)
            {
                return Task.FromResult(CallResult<FirebaseAuthException>.Error(ex));
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        protected void OnFirebaseAuthRefreshed(FirebaseAuth auth)
        {
            FirebaseAuthRefreshed?.Invoke(this, new FirebaseAuthEventArgs(auth));
        }

        private async Task<FirebaseAuth> ExecuteWithPostContentAsync(string googleUrl, string postContent)
        {
            string responseData = "N/A";

            try
            {
                var response = await client.PostAsync(
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
            catch (Exception ex)
            {
                AuthErrorReason errorReason = GetFailureReason(responseData);
                throw new FirebaseAuthException(googleUrl, postContent, responseData, ex, errorReason);
            }
        }

        private static AuthErrorReason GetFailureReason(string responseData)
        {
            var failureReason = AuthErrorReason.Undefined;
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
                            failureReason = AuthErrorReason.InvalidAccessToken;
                            break;

                        case "CREDENTIAL_TOO_OLD_LOGIN_AGAIN":
                            failureReason = AuthErrorReason.LoginCredentialsTooOld;
                            break;

                        case "OPERATION_NOT_ALLOWED":
                            failureReason = AuthErrorReason.OperationNotAllowed;
                            break;

                        //possible errors from Third Party Authentication using GoogleIdentityUrl
                        case "INVALID_PROVIDER_ID : Provider Id is not supported.":
                            failureReason = AuthErrorReason.InvalidProviderID;
                            break;
                        case "MISSING_REQUEST_URI":
                            failureReason = AuthErrorReason.MissingRequestURI;
                            break;
                        case "A system error has occurred - missing or invalid postBody":
                            failureReason = AuthErrorReason.SystemError;
                            break;

                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo) or Signin
                        case "INVALID_EMAIL":
                            failureReason = AuthErrorReason.InvalidEmailAddress;
                            break;
                        case "MISSING_PASSWORD":
                            failureReason = AuthErrorReason.MissingPassword;
                            break;

                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                        case "EMAIL_EXISTS":
                            failureReason = AuthErrorReason.EmailExists;
                            break;

                        //possible errors from Account Delete
                        case "USER_NOT_FOUND":
                            failureReason = AuthErrorReason.UserNotFound;
                            break;

                        //possible errors from Email/Password Signin
                        case "INVALID_PASSWORD":
                            failureReason = AuthErrorReason.WrongPassword;
                            break;
                        case "EMAIL_NOT_FOUND":
                            failureReason = AuthErrorReason.UnknownEmailAddress;
                            break;
                        case "USER_DISABLED":
                            failureReason = AuthErrorReason.UserDisabled;
                            break;

                        //possible errors from Email/Password Signin or Password Recovery or Email/Password Sign up using setAccountInfo
                        case "MISSING_EMAIL":
                            failureReason = AuthErrorReason.MissingEmail;
                            break;
                        case "RESET_PASSWORD_EXCEED_LIMIT":
                            failureReason = AuthErrorReason.ResetPasswordExceedLimit;
                            break;

                        //possible errors from Password Recovery
                        case "MISSING_REQ_TYPE":
                            failureReason = AuthErrorReason.MissingRequestType;
                            break;

                        //possible errors from Account Linking
                        case "INVALID_ID_TOKEN":
                            failureReason = AuthErrorReason.InvalidIDToken;
                            break;

                        //possible errors from Getting Linked Accounts
                        case "INVALID_IDENTIFIER":
                            failureReason = AuthErrorReason.InvalidIdentifier;
                            break;
                        case "MISSING_IDENTIFIER":
                            failureReason = AuthErrorReason.MissingIdentifier;
                            break;
                        case "FEDERATED_USER_ID_ALREADY_LINKED":
                            failureReason = AuthErrorReason.AlreadyLinked;
                            break;
                    }

                    if (failureReason == AuthErrorReason.Undefined)
                    {
                        //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                        if (errorData?.error?.message?.StartsWith("WEAK_PASSWORD :") ?? false) failureReason = AuthErrorReason.WeakPassword;
                        //possible errors from Email/Password Signin
                        else if (errorData?.error?.message?.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER :") ?? false) failureReason = AuthErrorReason.TooManyAttemptsTryLater;
                    }
                }
            }
            catch (JsonReaderException)
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

        private void CopyPropertiesLocally(FirebaseAuth auth)
        {
            if (auth != null)
            {
                if (auth.User != null) User = auth.User;
                if (auth.Created != default) Created = auth.Created;
                if (auth.ExpiresIn != default) ExpiresIn = auth.ExpiresIn;
                if (!string.IsNullOrEmpty(auth.RefreshToken)) RefreshToken = auth.RefreshToken;
                if (!string.IsNullOrEmpty(auth.FirebaseToken)) FirebaseToken = auth.FirebaseToken;
                SavePropertiesLocally();
            }
        }

        private void SavePropertiesLocally()
        {
            App.LocalDatabase.Set(Path.Combine(AuthRoot, "user"), JsonConvert.SerializeObject(User));
            App.LocalDatabase.Set(Path.Combine(AuthRoot, "created"), Helpers.EncodeDateTime(Created));
            App.LocalDatabase.Set(Path.Combine(AuthRoot, "expiresIn"), ExpiresIn.ToString());
            App.LocalDatabase.Set(Path.Combine(AuthRoot, "refreshToken"), RefreshToken);
            App.LocalDatabase.Set(Path.Combine(AuthRoot, "firebaseToken"), FirebaseToken);
        }

        private void RetainPropertiesLocally()
        {
            var rawUser = App.LocalDatabase.Get(Path.Combine(AuthRoot, "user"));
            User = rawUser == null ? default : JsonConvert.DeserializeObject<User>(rawUser);
            var rawCreated = App.LocalDatabase.Get(Path.Combine(AuthRoot, "created"));
            Created = rawCreated == null ? default : Helpers.DecodeDateTime(rawCreated, default);
            var rawExpiredIn = App.LocalDatabase.Get(Path.Combine(AuthRoot, "expiresIn"));
            ExpiresIn = rawExpiredIn == null ? default : int.Parse(App.LocalDatabase.Get(Path.Combine(AuthRoot, "expiresIn")));
            RefreshToken = App.LocalDatabase.Get(Path.Combine(AuthRoot, "refreshToken"));
            FirebaseToken = App.LocalDatabase.Get(Path.Combine(AuthRoot, "firebaseToken"));
        }

        private void PurgePropertiesLocally()
        {
            User = null;
            Created = default;
            ExpiresIn = 0;
            RefreshToken = null;
            FirebaseToken = null;
            App.LocalDatabase.DeletePath(AuthRoot);
        }
    }
}
