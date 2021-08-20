using Newtonsoft.Json;
using ObservableHelpers;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using RestfulFirebase.Serializers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Auth
{
    /// <summary>
    /// Provides firebase user authentication implementations.
    /// </summary>
    public class Session : SyncContext
    {
        #region Properties

        private const string Root = "auth";

        /// <summary>
        /// Gets the underlying <see cref="RestfulFirebaseApp"/> this module uses.
        /// </summary>
        public RestfulFirebaseApp App { get; }

        /// <summary>
        /// Gets the firebase token of the authenticated account which can be used for authenticated queries. 
        /// </summary>
        public string FirebaseToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "tok"), true);
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "tok"), value, true);
        }

        /// <summary>
        /// Gets the refresh token of the underlying service which can be used to get a new access token. 
        /// </summary>
        public string RefreshToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "ref"), true);
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "ref"), value, true);
        }

        /// <summary>
        /// Gets the number of seconds since the token is created.
        /// </summary>
        public int ExpiresIn
        {
            get => Serializer.Deserialize<int>(App.LocalDatabase.Get(Utils.UrlCombine(Root, "exp"), true));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "exp"), Serializer.Serialize(value), true);
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when this token was created.
        /// </summary>
        public DateTime Created
        {
            get => Serializer.Deserialize<DateTime>(App.LocalDatabase.Get(Utils.UrlCombine(Root, "ctd"), true));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "ctd"), Serializer.Serialize(value), true);
        }

        /// <summary>
        /// Gets the local id or the <c>UID</c> of the account.
        /// </summary>
        public string LocalId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "lid"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "lid"), value, true);
        }

        /// <summary>
        /// Gets the federated id of the account.
        /// </summary>
        public string FederatedId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "fid"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "fid"), value, true);
        }

        /// <summary>
        /// Gets the first name of the user.
        /// </summary>
        public string FirstName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "fname"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "fname"), value, true);
        }

        /// <summary>
        /// Gets the last name of the user.
        /// </summary>
        public string LastName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "lname"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "lname"), value, true);
        }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        public string DisplayName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "dname"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "dname"), value, true);
        }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        public string Email
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "email"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "email"), value, true);
        }

        /// <summary>
        /// Gets the email verfication status of the account.
        /// </summary>
        public bool IsEmailVerified
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "vmail"), true) == "1";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "vmail"), value ? "1" : "0", true);
        }

        /// <summary>
        /// Gets or sets the photo url of the account.
        /// </summary>
        public string PhotoUrl
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "purl"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "purl"), value, true);
        }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string PhoneNumber
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(Root, "pnum"), true) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(Root, "pnum"), value, true);
        }

        /// <summary>
        /// Gets whether this session exists.
        /// </summary>
        public bool Exist
        {
            get
            {
                return
                    !string.IsNullOrEmpty(FirebaseToken) &&
                    !string.IsNullOrEmpty(RefreshToken);
            }
        }

        /// <summary>
        /// Event raised on the current context when the authentication is refreshed.
        /// </summary>
        public event EventHandler AuthRefreshed;

        #endregion

        #region Initializers

        internal Session(RestfulFirebaseApp app)
        {
            SyncOperation.SetContext(app);

            App = app;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Change the email of the authenticated user.
        /// </summary>
        /// <param name="newEmail">
        /// The new email.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthEmailExistsException">
        /// The email address is already in use by another account.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task ChangeUserEmail(string newEmail)
        {
            var content = $"{{\"idToken\":\"{FirebaseToken}\",\"email\":\"{newEmail}\",\"returnSecureToken\":true}}";

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Change the password of the authenticated user.
        /// </summary>
        /// <param name="password">
        /// The new password.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthWeakPasswordException">
        /// The password must be 6 characters long or more.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task ChangeUserPassword(string password)
        {
            var content = $"{{\"idToken\":\"{FirebaseToken}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete the authenticated user.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task DeleteUser()
        {
            var responseData = "N/A";

            try
            {
                var content = $"{{ \"idToken\": \"{FirebaseToken}\" }}";
                var response = await App.Auth.GetClient().PostAsync(
                    new Uri(string.Format(FirebaseAuthApp.GoogleDeleteUserUrl, App.Config.ApiKey)),
                    new StringContent(content, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ExceptionHelpers.GetException(responseData, ex);
            }
        }

        /// <summary>
        /// Send email verification to the authenticated user`s email.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task SendEmailVerification()
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{token}\"}}";
            var responseData = "N/A";

            try
            {
                var response = await App.Auth.GetClient().PostAsync(
                    new Uri(string.Format(FirebaseAuthApp.GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                    new StringContent(content, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ExceptionHelpers.GetException(responseData, ex);
            }
        }

        /// <summary>
        /// Links the account with the provided <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email">
        /// The account`s email to be linked
        /// </param>
        /// <param name="password">
        /// The account`s password to be linked.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthLoginCredentialsTooOldException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthTokenExpiredException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthWeakPasswordException">
        /// The password must be 6 characters long or more.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task LinkAccounts(string email, string password)
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            var content = $"{{\"idToken\":\"{token}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleSetAccountUrl, content).ConfigureAwait(false);

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Links the account with oauth provided with <paramref name="authType"/> and <paramref name="oauthAccessToken"/>.
        /// </summary>
        /// <param name="authType">
        /// The <see cref="FirebaseAuthType"/> to be linked.
        /// </param>
        /// <param name="oauthAccessToken">
        /// The token of the provided <paramref name="authType"/> to be linked.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthOperationNotAllowedException">
        /// The corresponding provider is disabled for this project.
        /// </exception>
        /// <exception cref="AuthInvalidIDPResponseException">
        /// The supplied auth credential is malformed or has expired.
        /// </exception>
        /// <exception cref="AuthEmailExistsException">
        /// The email address is already in use by another account.
        /// </exception>
        /// <exception cref="AuthAlreadyLinkedException">
        /// This credential is already associated with a different user account.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task LinkAccounts(FirebaseAuthType authType, string oauthAccessToken)
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            var providerId = App.Auth.GetProviderId(authType);
            var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleIdentityUrl, content).ConfigureAwait(false);

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Unlinks the account with oauth provided with <paramref name="authType"/>.
        /// </summary>
        /// <param name="authType">
        /// The <see cref="FirebaseAuthType"/> to unlink.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// There is no user record corresponding to this identifier. The user may have been deleted.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task UnlinkAccounts(FirebaseAuthType authType)
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            string providerId;
            if (authType == FirebaseAuthType.EmailAndPassword)
            {
                providerId = authType.ToEnumString();
            }
            else
            {
                providerId = App.Auth.GetProviderId(authType);
            }

            var content = $"{{\"idToken\":\"{token}\",\"deleteProvider\":[\"{providerId}\"]}}";

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleSetAccountUrl, content).ConfigureAwait(false);

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all linked accounts of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>{<see cref="ProviderQueryResult"/>} proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthInvalidEmailAddressException">
        /// The email address is badly formatted.
        /// </exception>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task<ProviderQueryResult> GetLinkedAccounts()
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }
            var email = Email;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthMissingEmailException();
            }

            string responseData = "N/A";

            try
            {
                string content = $"{{\"identifier\":\"{email}\", \"continueUri\": \"http://localhost\"}}";
                var response = await App.Auth.GetClient().PostAsync(
                    new Uri(string.Format(FirebaseAuthApp.GoogleCreateAuthUrl, App.Config.ApiKey)),
                    new StringContent(content, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                ProviderQueryResult data = JsonConvert.DeserializeObject<ProviderQueryResult>(responseData);
                data.Email = email;

                return data;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ExceptionHelpers.GetException(responseData, ex);
            }
        }

        /// <summary>
        /// Gets the fresh token of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthTokenExpiredException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserDisabledException">
        /// The user account has been disabled by an administrator.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthInvalidRefreshTokenException">
        /// An invalid refresh token is provided.
        /// </exception>
        /// <exception cref="AuthInvalidJSONReceivedException">
        /// Invalid JSON payload received.
        /// </exception>
        /// <exception cref="AuthMissingRefreshTokenException">
        /// No refresh token provided.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task<string> GetFreshToken()
        {
            var token = RefreshToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            if (IsExpired())
            {
                var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{token}\"}}";
                var responseData = "N/A";

                try
                {
                    HttpResponseMessage response = null;
                    response = await App.Auth.GetClient().PostAsync(
                        new Uri(string.Format(FirebaseAuthApp.GoogleRefreshAuth, App.Config.ApiKey)),
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

                    UpdateAuth(auth);

                    OnAuthRefreshed();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw ExceptionHelpers.GetException(responseData, ex);
                }
            }

            return FirebaseToken;
        }

        /// <summary>
        /// Refreshes the token of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthTokenExpiredException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserDisabledException">
        /// The user account has been disabled by an administrator.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthInvalidRefreshTokenException">
        /// An invalid refresh token is provided.
        /// </exception>
        /// <exception cref="AuthInvalidJSONReceivedException">
        /// Invalid JSON payload received.
        /// </exception>
        /// <exception cref="AuthMissingRefreshTokenException">
        /// No refresh token provided.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task RefreshAuth()
        {
            await GetFreshToken().ConfigureAwait(false);

            var auth = new FirebaseAuth()
            {
                FirebaseToken = FirebaseToken,
                RefreshToken = RefreshToken,
                ExpiresIn = ExpiresIn,
                Created = Created
            };

            await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the accounts profile provided with <paramref name="displayName"/> and <paramref name="photoUrl"/>.
        /// </summary>
        /// <param name="displayName">
        /// The new display name of the account.
        /// </param>
        /// <param name="photoUrl">
        /// The new photo url of the account.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        public async Task UpdateProfile(string displayName, string photoUrl)
        {
            var token = FirebaseToken;
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthNotAuthenticatedException();
            }

            StringBuilder sb = new StringBuilder($"{{\"idToken\":\"{token}\"");
            if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(photoUrl))
            {
                sb.Append($",\"displayName\":\"{displayName}\",\"photoUrl\":\"{photoUrl}\"");
            }
            else if (!string.IsNullOrWhiteSpace(displayName))
            {
                sb.Append($",\"displayName\":\"{displayName}\"");
                sb.Append($",\"deleteAttribute\":[\"{FirebaseAuthApp.ProfileDeletePhotoUrl}\"]");
            }
            else if (!string.IsNullOrWhiteSpace(photoUrl))
            {
                sb.Append($",\"photoUrl\":\"{photoUrl}\"");
                sb.Append($",\"deleteAttribute\":[\"{FirebaseAuthApp.ProfileDeleteDisplayName}\"]");
            }
            else
            {
                sb.Append($",\"deleteAttribute\":[\"{FirebaseAuthApp.ProfileDeleteDisplayName}\",\"{FirebaseAuthApp.ProfileDeletePhotoUrl}\"]");
            }

            sb.Append($",\"returnSecureToken\":true}}");

            var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleSetAccountUrl, sb.ToString()).ConfigureAwait(false);

            UpdateAuth(auth);
        }

        /// <summary>
        /// Sign out the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> proxy of the specified task.
        /// </returns>
        public async Task Signout()
        {
            await Task.Run(delegate
            {
                Purge();
                App.Database.Flush();
                App.Auth.InvokeAuthenticationEvents();
            });
        }

        /// <summary>
        /// Check if the token is expired.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the token is expired; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExpired()
        {
            return DateTime.Now > Created.AddSeconds(ExpiresIn - 10);
        }

        /// <summary>
        /// Invokes <see cref="AuthRefreshed"/> event into the current context.
        /// </summary>
        internal void OnAuthRefreshed()
        {
            App.Auth.OnAuthRefreshed();
            ContextPost(delegate
            {
                AuthRefreshed?.Invoke(this, new EventArgs());
            });
        }

        internal void UpdateAuth(FirebaseAuth auth)
        {
            if (!string.IsNullOrEmpty(auth.FirebaseToken)) FirebaseToken = auth.FirebaseToken;
            if (!string.IsNullOrEmpty(auth.RefreshToken)) RefreshToken = auth.RefreshToken;
            if (auth.ExpiresIn.HasValue) ExpiresIn = auth.ExpiresIn.Value;
            if (auth.Created.HasValue) Created = auth.Created.Value;
            if (auth.User != null) UpdateUserInfo(auth.User);
        }

        internal void UpdateUserInfo(User user)
        {
            LocalId = user.LocalId;
            FederatedId = user.FederatedId;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DisplayName = user.DisplayName;
            Email = user.Email;
            IsEmailVerified = user.IsEmailVerified;
            PhotoUrl = user.PhoneNumber;
            PhoneNumber = user.PhoneNumber;
        }

        internal void Purge()
        {
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "ctd"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "exp"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "ref"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "tok"), true);

            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "lid"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "fid"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "fname"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "lname"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "dname"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "email"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "vmail"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "purl"), true);
            App.LocalDatabase.Delete(Utils.UrlCombine(Root, "pnum"), true);
        }

        #endregion
    }
}
