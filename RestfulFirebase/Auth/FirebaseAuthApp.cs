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
using ObservableHelpers.Serializers;
using System.Text.Json;

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

        public event Action FirebaseAuthRefreshed;

        public event Action OnAuthenticated;

        internal FirebaseAuthApp(RestfulFirebaseApp firebaseApp)
        {
            App = firebaseApp;
            client = new HttpClient();
            RetainPropertiesLocally();
        }

        public async Task<CallResult> CreateUserWithEmailAndPasswordAsync(string email, string password, string displayName = "", bool sendVerificationEmail = false)
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

                InvokeOnAuthenticated();

                if (sendVerificationEmail)
                {
                    //send verification email
                    await SendEmailVerificationAsync().ConfigureAwait(false);
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInWithCustomTokenAsync(string customToken)
        {
            try
            {
                string content = $"{{\"token\":\"{customToken}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleCustomAuthUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                InvokeOnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInWithOAuthAsync(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                var providerId = GetProviderId(authType);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                InvokeOnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInWithOAuthTwitterTokenAsync(string oauthAccessToken, string oauthTokenSecret)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Twitter);
                var content = $"{{\"postBody\":\"access_token={oauthAccessToken}&oauth_token_secret={oauthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                InvokeOnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInWithGoogleIdTokenAsync(string idToken)
        {
            try
            {
                var providerId = GetProviderId(FirebaseAuthType.Google);
                var content = $"{{\"postBody\":\"id_token={idToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                InvokeOnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInWithEmailAndPasswordAsync(string email, string password, string tenantId = null)
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

                InvokeOnAuthenticated();

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SignInAnonymouslyAsync()
        {
            try
            {
                var content = $"{{\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSignUpUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                InvokeOnAuthenticated();

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
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleUpdateUserPassword, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> SendPasswordResetEmailAsync(string email)
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

        public async Task<CallResult> DeleteUserAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

        public async Task<CallResult> SendEmailVerificationAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{FirebaseToken}\"}}";
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

        public async Task<CallResult> LinkAccountsAsync(string email, string password)
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleSetAccountUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> LinkAccountsAsync(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var providerId = GetProviderId(authType);
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await ExecuteWithPostContentAsync(GoogleIdentityUrl, content).ConfigureAwait(false);

                CopyPropertiesLocally(auth);
                var refreshResult = await RefreshUserDetailsAsync();
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult> UnlinkAccountsAsync(FirebaseAuthType authType)
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult<ProviderQueryResult>> GetLinkedAccountsAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                    data = JsonSerializer.Deserialize<ProviderQueryResult>(responseData, Utils.JsonSerializerOptions);
                    data.Email = User.Email;
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

        public async Task<CallResult> RefreshUserDetailsAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                    var resultJson = JsonDocument.Parse(responseData);
                    var user = JsonSerializer.Deserialize<User>(resultJson.RootElement.GetProperty("users").EnumerateObject().First().Value.ToString(), Utils.JsonSerializerOptions);
                    User = user;
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

        public async Task<CallResult> RefreshAuthAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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
                        var refreshAuth = JsonSerializer.Deserialize<RefreshAuth>(responseData, Utils.JsonSerializerOptions);

                        var auth = new FirebaseAuth
                        {
                            ExpiresIn = refreshAuth.ExpiresIn,
                            RefreshToken = refreshAuth.RefreshToken,
                            FirebaseToken = refreshAuth.AccessToken
                        };

                        CopyPropertiesLocally(auth);
                        InvokeFirebaseAuthRefreshed();
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

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public async Task<CallResult<string>> GetFreshTokenAsync()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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
                        var refreshAuth = JsonSerializer.Deserialize<RefreshAuth>(responseData, Utils.JsonSerializerOptions);

                        var auth = new FirebaseAuth
                        {
                            ExpiresIn = refreshAuth.ExpiresIn,
                            RefreshToken = refreshAuth.RefreshToken,
                            FirebaseToken = refreshAuth.AccessToken
                        };

                        CopyPropertiesLocally(auth);
                        InvokeFirebaseAuthRefreshed();
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

                return CallResult.Success<string>(FirebaseToken);
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error<string>(ex);
            }
        }

        public async Task<CallResult> UpdateProfileAsync(string displayName, string photoUrl)
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        public Task<CallResult> Signout()
        {
            try
            {
                if (!Authenticated) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));
                PurgePropertiesLocally();

                return Task.FromResult(CallResult.Success());
            }
            catch (FirebaseException ex)
            {
                return Task.FromResult(CallResult.Error(ex));
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        protected void InvokeFirebaseAuthRefreshed()
        {
            FirebaseAuthRefreshed?.Invoke();
        }

        protected void InvokeOnAuthenticated()
        {
            OnAuthenticated?.Invoke();
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

                var user = JsonSerializer.Deserialize<User>(responseData, Utils.JsonSerializerOptions);
                var auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, Utils.JsonSerializerOptions);

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
                    errorData = Utils.JsonDeserializeAnonymousType(responseData, errorData, Utils.JsonSerializerOptions);

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
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "lid"), User.LocalId);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "fid"), User.FederatedId);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "fname"), User.FirstName);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "lname"), User.LastName);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "dname"), User.DisplayName);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "email"), User.Email);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "vmail"), User.IsEmailVerified ? "1" : "0");
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "purl"), User.PhotoUrl);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "pnum"), User.PhoneNumber);

            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "created"), Serializer.Serialize(Created));
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "expiresIn"), ExpiresIn.ToString());
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "refreshToken"), RefreshToken);
            App.LocalDatabase.Set(Utils.CombineUrl(AuthRoot, "firebaseToken"), FirebaseToken);
        }

        private void RetainPropertiesLocally()
        {
            var lid = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "lid"));
            var fid = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "fid"));
            var fname = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "fname"));
            var lname = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "lname"));
            var dname = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "dname"));
            var email = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "email"));
            var vmail = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "vmail"));
            var purl = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "purl"));
            var pnum = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "pnum"));

            User = new User()
            {
                LocalId = lid,
                FederatedId = fid,
                FirstName = fname,
                LastName = lname,
                DisplayName = dname,
                Email = email,
                IsEmailVerified = vmail == "1",
                PhotoUrl = purl,
                PhoneNumber = pnum
            };
            var rawCreated = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "created"));
            Created = rawCreated == null ? default : Serializer.Deserialize<DateTime>(rawCreated, default);
            var rawExpiredIn = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "expiresIn"));
            ExpiresIn = rawExpiredIn == null ? default : int.Parse(App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "expiresIn")));
            RefreshToken = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "refreshToken"));
            FirebaseToken = App.LocalDatabase.Get(Utils.CombineUrl(AuthRoot, "firebaseToken"));
        }

        private void PurgePropertiesLocally()
        {
            User = null;
            Created = default;
            ExpiresIn = 0;
            RefreshToken = null;
            FirebaseToken = null;

            App.LocalDatabase.Delete(Utils.CombineUrl(AuthRoot, "user"));
            App.LocalDatabase.Delete(Utils.CombineUrl(AuthRoot, "created"));
            App.LocalDatabase.Delete(Utils.CombineUrl(AuthRoot, "expiresIn"));
            App.LocalDatabase.Delete(Utils.CombineUrl(AuthRoot, "refreshToken"));
            App.LocalDatabase.Delete(Utils.CombineUrl(AuthRoot, "firebaseToken"));
        }
    }
}
