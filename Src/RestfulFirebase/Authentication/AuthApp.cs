using RestfulFirebase.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Http;
using RestfulFirebase.Exceptions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DisposableHelpers.Attributes;
using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Internals;

namespace RestfulFirebase.Auth;

/// <summary>
/// App module that provides firebase authentication implementations.
/// </summary>
[Disposable]
public partial class AuthApp
{
    #region Methods


    /// <summary>
    /// Send password reset email to the existing account provided with the <paramref name="email"/>.
    /// </summary>
    /// <param name="email">
    /// The email of the user to send the password reset.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthEmailNotFoundException">
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
    public async Task SendPasswordResetEmail(string email)
    {
        var responseData = "N/A";

        try
        {
            var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"}}";
            var response = await GetClient().PostAsync(
                new Uri(string.Format(GoogleGetConfirmationCodeUrl, App.Config.ApiKey)),
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
    /// Copies authentication from another <see cref="RestfulFirebaseApp"/>.
    /// </summary>
    /// <param name="app">
    /// The <see cref="RestfulFirebaseApp"/> to copy from
    /// </param>
    public void CopyAuthenticationFrom(RestfulFirebaseApp app)
    {
        session.CopyAuthenticationFrom(app);
    }

    internal void OnAuthRefreshed()
    {
        AuthRefreshed?.Invoke(this, new EventArgs());
    }

    internal void InvokeAuthenticationEvents()
    {
        if (IsAuthenticated)
        {
            Authenticated?.Invoke(this, new EventArgs());
        }
        else
        {
            Unauthenticated?.Invoke(this, new EventArgs());
        }
        AuthenticationChanges?.Invoke(this, new AuthenticationChangesEventArgs(IsAuthenticated));
    }


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
                new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);
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
                    new CancellationTokenSource(App.Config.AuthRequestTimeout).Token).ConfigureAwait(false);

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

    #endregion

    #region Disposable Members

    /// <summary>
    /// The dispose logic.
    /// </summary>
    /// <param name = "disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            client?.Dispose();
        }
    }

    #endregion
}
