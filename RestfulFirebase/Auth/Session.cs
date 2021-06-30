using Newtonsoft.Json;
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
    public class Session
    {
        #region Properties

        private const string AuthRoot = "auth";

        /// <summary>
        /// Gets the underlying <see cref="RestfulFirebaseApp"/> this module uses.
        /// </summary>
        public RestfulFirebaseApp App { get; }

        /// <summary>
        /// Gets the firebase token of the authenticated account which can be used for authenticated queries. 
        /// </summary>
        public string FirebaseToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "tok"));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "tok"), value);
        }

        /// <summary>
        /// Gets the refresh token of the underlying service which can be used to get a new access token. 
        /// </summary>
        public string RefreshToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "ref"));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "ref"), value);
        }

        /// <summary>
        /// Gets the number of seconds since the token is created.
        /// </summary>
        public int ExpiresIn
        {
            get => Serializer.Deserialize<int>(App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "exp")));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "exp"), Serializer.Serialize(value));
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when this token was created.
        /// </summary>
        public DateTime Created
        {
            get => Serializer.Deserialize<DateTime>(App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "ctd")));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "ctd"), Serializer.Serialize(value));
        }

        /// <summary>
        /// Gets the local id or the <c>UID</c> of the account.
        /// </summary>
        public string LocalId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "lid")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "lid"), value);
        }

        /// <summary>
        /// Gets the federated id of the account.
        /// </summary>
        public string FederatedId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "fid")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "fid"), value);
        }

        /// <summary>
        /// Gets the first name of the user.
        /// </summary>
        public string FirstName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "fname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "fname"), value);
        }

        /// <summary>
        /// Gets the last name of the user.
        /// </summary>
        public string LastName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "lname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "lname"), value);
        }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        public string DisplayName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "dname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "dname"), value);
        }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        public string Email
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "email")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "email"), value);
        }

        /// <summary>
        /// Gets the email verfication status of the account.
        /// </summary>
        public bool IsEmailVerified
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "vmail")) == "1";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "vmail"), value ? "1" : "0");
        }

        /// <summary>
        /// Gets or sets the photo url of the account.
        /// </summary>
        public string PhotoUrl
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "purl")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "purl"), value);
        }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string PhoneNumber
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "pnum")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "pnum"), value);
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

        #endregion

        #region Initializers

        internal Session(RestfulFirebaseApp app)
        {
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
        /// The <see cref="FirebaseAuth"/>.
        /// </returns>
        public async Task<CallResult> ChangeUserEmail(string newEmail)
        {
            try
            {
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"email\":\"{newEmail}\",\"returnSecureToken\":true}}";

                var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Change the password of the authenticated user.
        /// </summary>
        /// <param name="password">
        /// The new password.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> ChangeUserPassword(string password)
        {
            try
            {
                var content = $"{{\"idToken\":\"{FirebaseToken}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Delete the authenticated user
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> DeleteUser()
        {
            try
            {
                var content = $"{{ \"idToken\": \"{FirebaseToken}\" }}";
                var responseData = "N/A";

                try
                {
                    var response = await App.Auth.GetClient().PostAsync(
                        new Uri(string.Format(FirebaseAuthApp.GoogleDeleteUserUrl, App.Config.ApiKey)),
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
                    FirebaseExceptionReason errorReason = FirebaseAuthApp.GetFailureReason(responseData);
                    throw new FirebaseException(errorReason, ex);
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Send email verification to the authenticated user`s email.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> SendEmailVerification()
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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
                catch (OperationCanceledException ex)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                }
                catch (Exception ex)
                {
                    FirebaseExceptionReason errorReason = FirebaseAuthApp.GetFailureReason(responseData);
                    throw new FirebaseException(errorReason, ex);
                }

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
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
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> LinkAccounts(string email, string password)
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var content = $"{{\"idToken\":\"{token}\",\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

                var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleSetAccountUrl, content).ConfigureAwait(false);

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
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
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> LinkAccounts(FirebaseAuthType authType, string oauthAccessToken)
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

                var providerId = App.Auth.GetProviderId(authType);
                var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

                var auth = await App.Auth.ExecuteWithPostContent(FirebaseAuthApp.GoogleIdentityUrl, content).ConfigureAwait(false);

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Unlinks the account with oauth provided with <paramref name="authType"/>.
        /// </summary>
        /// <param name="authType">
        /// The <see cref="FirebaseAuthType"/> to unlink.
        /// </param>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> UnlinkAccounts(FirebaseAuthType authType)
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Gets all linked accounts of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult<ProviderQueryResult>> GetLinkedAccounts()
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));
                var email = Email;
                if (string.IsNullOrEmpty(email)) throw new FirebaseException(FirebaseExceptionReason.AuthMissingEmail, new Exception("Email not found"));

                string content = $"{{\"identifier\":\"{email}\", \"continueUri\": \"http://localhost\"}}";
                string responseData = "N/A";

                ProviderQueryResult data;

                try
                {
                    var response = await App.Auth.GetClient().PostAsync(
                        new Uri(string.Format(FirebaseAuthApp.GoogleCreateAuthUrl, App.Config.ApiKey)),
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
                    FirebaseExceptionReason errorReason = FirebaseAuthApp.GetFailureReason(responseData);
                    throw new FirebaseException(errorReason, ex);
                }

                return CallResult.Success<ProviderQueryResult>(data);
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error<ProviderQueryResult>(ex);
            }
        }

        /// <summary>
        /// Gets the fresh token of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult<string>> GetFreshToken()
        {
            try
            {
                var token = RefreshToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                        App.Auth.OnAuthRefreshed();
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

                return CallResult.Success(FirebaseToken);
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error<string>(ex);
            }
        }

        /// <summary>
        /// Refreshes the token of the authenticated account.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> RefreshAuth()
        {
            try
            {
                var refresh = await GetFreshToken().ConfigureAwait(false);
                if (!refresh.IsSuccess) return refresh;

                var auth = new FirebaseAuth()
                {
                    FirebaseToken = FirebaseToken,
                    RefreshToken = RefreshToken,
                    ExpiresIn = ExpiresIn,
                    Created = Created
                };

                var refreshResult = await App.Auth.RefreshUserInfo(auth).ConfigureAwait(false);
                if (!refreshResult.IsSuccess) return refreshResult;

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
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
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> UpdateProfile(string displayName, string photoUrl)
        {
            try
            {
                var token = FirebaseToken;
                if (string.IsNullOrEmpty(token)) throw new FirebaseException(FirebaseExceptionReason.AuthNotAuthenticated, new Exception("Not authenticated"));

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

                return CallResult.Success();
            }
            catch (FirebaseException ex)
            {
                return CallResult.Error(ex);
            }
        }

        /// <summary>
        /// Sign out the authentcated account.
        /// </summary>
        /// <returns>
        /// The <see cref="CallResult"/> of the specified task.
        /// </returns>
        public async Task<CallResult> Signout()
        {
            try
            {
                Purge();

                return await Task.FromResult(CallResult.Success()).ConfigureAwait(false);
            }
            catch (FirebaseException ex)
            {
                return await Task.FromResult(CallResult.Error(ex)).ConfigureAwait(false);
            }
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
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "ctd"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "exp"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "ref"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "tok"));

            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "lid"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "fid"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "fname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "lname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "dname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "email"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "vmail"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "purl"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "pnum"));
        }

        #endregion
    }
}
