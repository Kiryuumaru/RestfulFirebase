using ObservableHelpers;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers.Utilities;
using RestfulFirebase.Local;
using SynchronizationContextHelpers;
using SerializerHelpers;
using System.Text.Json;

namespace RestfulFirebase.Auth;

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
    public string? FirebaseToken { get; private set; }

    /// <summary>
    /// Gets the refresh token of the underlying service which can be used to get a new access token. 
    /// </summary>
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Gets the number of seconds since the token is created.
    /// </summary>
    public int ExpiresIn { get; private set; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> when this token was created.
    /// </summary>
    public DateTime Created { get; private set; }

    /// <summary>
    /// Gets the local id or the <c>UID</c> of the account.
    /// </summary>
    public string? LocalId { get; private set; }

    /// <summary>
    /// Gets the federated id of the account.
    /// </summary>
    public string? FederatedId { get; private set; }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the email of the user.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the email verfication status of the account.
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Gets or sets the photo url of the account.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; private set; }

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
    public event EventHandler? AuthRefreshed;

    #endregion

    #region Initializers

    internal Session(RestfulFirebaseApp app)
    {
        SyncOperation.SetContext(app);

        App = app;

        App.Config.ImmediatePropertyChanged +=  Config_ImmediatePropertyChanged;

        Fetch(App.Config.CachedCustomAuthLocalDatabase ?? App.Config.CachedLocalDatabase);
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

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

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

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleUpdateUserPassword, content).ConfigureAwait(false);

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
                new Uri(string.Format(AuthApp.GoogleDeleteUserUrl, App.Config.ApiKey)),
                new StringContent(content, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.CachedAuthRequestTimeout).Token).ConfigureAwait(false);
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
                new Uri(string.Format(AuthApp.GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
                new StringContent(content, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.CachedAuthRequestTimeout).Token).ConfigureAwait(false);
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

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleSetAccountUrl, content).ConfigureAwait(false);

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

        var providerId = AuthApp.GetProviderId(authType);
        var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={oauthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleIdentityUrl, content).ConfigureAwait(false);

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

        string? providerId;
        if (authType == FirebaseAuthType.EmailAndPassword)
        {
            providerId = authType.ToEnumString();
        }
        else
        {
            providerId = AuthApp.GetProviderId(authType);
        }

        if (string.IsNullOrEmpty(providerId))
        {
            throw new AuthUndefinedException();
        }

        var content = $"{{\"idToken\":\"{token}\",\"deleteProvider\":[\"{providerId}\"]}}";

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleSetAccountUrl, content).ConfigureAwait(false);

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
                new Uri(string.Format(AuthApp.GoogleCreateAuthUrl, App.Config.ApiKey)),
                new StringContent(content, Encoding.UTF8, "Application/json"),
                new CancellationTokenSource(App.Config.CachedAuthRequestTimeout).Token).ConfigureAwait(false);
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            ProviderQueryResult? data = JsonSerializer.Deserialize<ProviderQueryResult>(responseData, RestfulFirebaseApp.DefaultJsonSerializerOption);
            
            if (data == null)
            {
                throw new AuthUndefinedException();
            }
            
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
                HttpResponseMessage response = await App.Auth.GetClient().PostAsync(
                    new Uri(string.Format(AuthApp.GoogleRefreshAuth, App.Config.ApiKey)),
                    new StringContent(content, Encoding.UTF8, "Application/json"),
                    new CancellationTokenSource(App.Config.CachedAuthRequestTimeout).Token).ConfigureAwait(false);

                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var refreshAuth = JsonSerializer.Deserialize<RefreshAuth>(responseData, RestfulFirebaseApp.DefaultJsonSerializerOption);

                if (refreshAuth == null)
                {
                    throw new AuthUndefinedException();
                }

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

        if (FirebaseToken == null)
        {
            throw new AuthNotAuthenticatedException();
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
            ExpiresIn = ExpiresIn
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

        StringBuilder sb = new($"{{\"idToken\":\"{token}\"");
        if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(photoUrl))
        {
            sb.Append($",\"displayName\":\"{displayName}\",\"photoUrl\":\"{photoUrl}\"");
        }
        else if (!string.IsNullOrWhiteSpace(displayName))
        {
            sb.Append($",\"displayName\":\"{displayName}\"");
            sb.Append($",\"deleteAttribute\":[\"{AuthApp.ProfileDeletePhotoUrl}\"]");
        }
        else if (!string.IsNullOrWhiteSpace(photoUrl))
        {
            sb.Append($",\"photoUrl\":\"{photoUrl}\"");
            sb.Append($",\"deleteAttribute\":[\"{AuthApp.ProfileDeleteDisplayName}\"]");
        }
        else
        {
            sb.Append($",\"deleteAttribute\":[\"{AuthApp.ProfileDeleteDisplayName}\",\"{AuthApp.ProfileDeletePhotoUrl}\"]");
        }

        sb.Append($",\"returnSecureToken\":true}}");

        var auth = await App.Auth.ExecuteAuthWithPostContent(AuthApp.GoogleSetAccountUrl, sb.ToString()).ConfigureAwait(false);

        UpdateAuth(auth);
    }

    /// <summary>
    /// Sign out the authenticated account.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    public void Signout()
    {
        Purge();
        App.Database.Flush();
        App.Auth.InvokeAuthenticationEvents();
    }

    /// <summary>
    /// Check if the token is expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the token is expired; otherwise, <c>false</c>.
    /// </returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow > Created.AddSeconds(ExpiresIn - 10);
    }

    /// <summary>
    /// Copies authentication from another <see cref="RestfulFirebaseApp"/>.
    /// </summary>
    /// <param name="app">
    /// The <see cref="RestfulFirebaseApp"/> to copy from
    /// </param>
    public void CopyAuthenticationFrom(RestfulFirebaseApp app)
    {
        ILocalDatabase localDatabase = app.Config.CachedCustomAuthLocalDatabase ?? app.Config.CachedLocalDatabase;
        Fetch(localDatabase);
        Store(localDatabase);
    }

    internal void OnAuthRefreshed()
    {
        App.Auth.OnAuthRefreshed();
        ContextPost(delegate
        {
            AuthRefreshed?.Invoke(this, new EventArgs());
        });
    }

    internal void UpdateAuth(FirebaseAuth? auth)
    {
        if (auth == null)
        {
            return;
        }
        if (FirebaseToken != auth.FirebaseToken) Created = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(auth.FirebaseToken)) FirebaseToken = auth.FirebaseToken;
        if (!string.IsNullOrEmpty(auth.RefreshToken)) RefreshToken = auth.RefreshToken;
        if (auth.ExpiresIn.HasValue) ExpiresIn = auth.ExpiresIn.Value;
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
        Store(App.Config.CachedCustomAuthLocalDatabase ?? App.Config.CachedLocalDatabase);
    }

    internal void Purge()
    {
        FirebaseToken = default;
        RefreshToken = default;
        ExpiresIn = default;
        Created = default;
        LocalId = default;
        FederatedId = default;
        FirstName = default;
        LastName = default;
        DisplayName = default;
        Email = default;
        IsEmailVerified = default;
        PhotoUrl = default;
        PhoneNumber = default;

        App.LocalDatabase.InternalDelete(App.Config.CachedCustomAuthLocalDatabase ?? App.Config.CachedLocalDatabase, new string[] { Root });
    }

    internal void Fetch(ILocalDatabase? localDatabase)
    {
        if (localDatabase == null)
        {
            return;
        }

        var auth = App.LocalDatabase.InternalGetValue(localDatabase, new string[] { Root }) ?? "";

        FirebaseToken = BlobSerializer.GetValue(auth, "tok");
        RefreshToken = BlobSerializer.GetValue(auth, "ref");
        ExpiresIn = Serializer.Deserialize<int>(BlobSerializer.GetValue(auth, "exp"));
        Created = Serializer.Deserialize<DateTime>(BlobSerializer.GetValue(auth, "ctd"));
        LocalId = BlobSerializer.GetValue(auth, "lid");
        FederatedId = BlobSerializer.GetValue(auth, "fid");
        FirstName = BlobSerializer.GetValue(auth, "fname");
        LastName = BlobSerializer.GetValue(auth, "lname");
        DisplayName = BlobSerializer.GetValue(auth, "dname");
        Email = BlobSerializer.GetValue(auth, "email");
        IsEmailVerified = Serializer.Deserialize<bool>(BlobSerializer.GetValue(auth, "vmail"));
        PhotoUrl = BlobSerializer.GetValue(auth, "purl");
        PhoneNumber = BlobSerializer.GetValue(auth, "pnum");
    }

    internal void Store(ILocalDatabase localDatabase)
    {
        var auth = "";

        auth = BlobSerializer.SetValue(auth, "tok", FirebaseToken);
        auth = BlobSerializer.SetValue(auth, "ref", RefreshToken);
        auth = BlobSerializer.SetValue(auth, "exp", Serializer.Serialize(ExpiresIn));
        auth = BlobSerializer.SetValue(auth, "ctd", Serializer.Serialize(Created));
        auth = BlobSerializer.SetValue(auth, "lid", LocalId);
        auth = BlobSerializer.SetValue(auth, "fid", FederatedId);
        auth = BlobSerializer.SetValue(auth, "fname", FirstName);
        auth = BlobSerializer.SetValue(auth, "lname", LastName);
        auth = BlobSerializer.SetValue(auth, "dname", DisplayName);
        auth = BlobSerializer.SetValue(auth, "email", Email);
        auth = BlobSerializer.SetValue(auth, "vmail", Serializer.Serialize(IsEmailVerified));
        auth = BlobSerializer.SetValue(auth, "purl", PhotoUrl);
        auth = BlobSerializer.SetValue(auth, "pnum", PhoneNumber);

        App.LocalDatabase.InternalSetValue(localDatabase, auth, new string[] { Root });
    }

    private void Config_ImmediatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.Config.LocalDatabase) ||
            e.PropertyName == nameof(App.Config.CustomAuthLocalDatabase))
        {
            Fetch(App.Config.CustomAuthLocalDatabase ?? App.Config.LocalDatabase);
        }
    }

    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            App.Config.ImmediatePropertyChanged -= Config_ImmediatePropertyChanged;
        }
        base.Dispose(disposing);
    }

    #endregion
}
