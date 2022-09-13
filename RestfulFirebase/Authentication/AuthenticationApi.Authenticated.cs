using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Requests;
using System.Linq;
using System.Text.Json.Serialization;
using RestfulFirebase.Common.Requests;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static partial class Authentication
{
    /// <summary>
    /// Send email verification to the authenticated user`s email.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
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
    public static async Task SendEmailVerification(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{token}\"}}";

        await ExecuteWithPostContent(request, GoogleGetConfirmationCodeUrl, content);
    }

    /// <summary>
    /// Change the email of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserEmailRequest.NewEmail"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
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
    public static async Task ChangeUserEmail(ChangeUserEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewEmail);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"email\":\"{request.NewEmail}\",\"returnSecureToken\":true}}";

        await ExecuteAuthWithPostContent(request, GoogleUpdateUser, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Change the password of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserPasswordRequest.NewPassword"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
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
    public static async Task ChangeUserPassword(ChangeUserPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewPassword);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"password\":\"{request.NewPassword}\",\"returnSecureToken\":true}}";

        await ExecuteAuthWithPostContent(request, GoogleUpdateUser, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Update the accounts profile provided with display name and photo URL.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task UpdateProfile(UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        StringBuilder sb = new($"{{\"idToken\":\"{token}\"");
        if (!string.IsNullOrWhiteSpace(request.DisplayName) && !string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            sb.Append($",\"displayName\":\"{request.DisplayName}\",\"photoUrl\":\"{request.PhotoUrl}\"");
        }
        else if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            sb.Append($",\"displayName\":\"{request.DisplayName}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeletePhotoUrl}\"]");
        }
        else if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            sb.Append($",\"photoUrl\":\"{request.PhotoUrl}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\"]");
        }
        else
        {
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\",\"{ProfileDeletePhotoUrl}\"]");
        }

        sb.Append($",\"returnSecureToken\":true}}");

        string content = sb.ToString();

        await ExecuteAuthWithPostContent(request, GoogleSetAccountUrl, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Delete the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    public static async Task DeleteUser(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        var content = $"{{ \"idToken\": \"{token}\" }}";

        await ExecuteWithPostContent(request, GoogleDeleteUserUrl, content);
    }

    /// <summary>
    /// Links the account with the provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/>,
    /// <see cref="LinkAccountRequest.Email"/> and
    /// <see cref="LinkAccountRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
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
    public static async Task LinkAccount(LinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",\"returnSecureToken\":true}}";

        await ExecuteAuthWithPostContent(request, GoogleSetAccountUrl, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }


    /// <summary>
    /// Links the account with oauth provided with auth type and oauth access token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/>,
    /// <see cref="LinkOAuthAccountRequest.AuthType"/> and
    /// <see cref="LinkOAuthAccountRequest.OAuthAccessToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
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
    public static async Task LinkAccount(LinkOAuthAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);

        string token = await GetFreshToken(request);

        var providerId = GetProviderId(request.AuthType.Value);
        var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={request.OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";
        
        await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Unlinks the account with oauth provided with auth type.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="UnlinkAccountRequest.AuthType"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
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
    public static async Task UnlinkAccounts(UnlinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);

        string token = await GetFreshToken(request);

        string? providerId;
        if (request.AuthType.Value == FirebaseAuthType.EmailAndPassword)
        {
            providerId = request.AuthType.Value.ToEnumString();
        }
        else
        {
            providerId = GetProviderId(request.AuthType.Value);
        }

        if (string.IsNullOrEmpty(providerId))
        {
            throw new AuthUndefinedException();
        }

        var content = $"{{\"idToken\":\"{token}\",\"deleteProvider\":[\"{providerId}\"]}}";

        await ExecuteAuthWithPostContent(request, GoogleSetAccountUrl, content, CamelCaseJsonSerializerOption);

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Gets all linked accounts of the authenticated account.
    /// </summary>
    /// <returns>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// The <see cref="Task"/>{<see cref="ProviderQueryResult"/>} proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidEmailAddressException">
    /// The email address is badly formatted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<ProviderQueryResult> GetLinkedAccounts(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string content = $"{{\"identifier\":\"{request.FirebaseUser.Email}\", \"continueUri\": \"http://localhost\"}}";

        ProviderQueryResult? data = await ExecuteWithPostContent<ProviderQueryResult>(request, GoogleCreateAuthUrl, content, CamelCaseJsonSerializerOption);

        if (data == null)
        {
            throw new AuthUndefinedException();
        }

        data.Email = request.FirebaseUser.Email;

        return data;
    }

    /// <summary>
    /// Gets the fresh token of the authenticated account.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
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
    public static async Task<string> GetFreshToken(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        if (request.FirebaseUser.IsExpired())
        {
            var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{request.FirebaseUser.RefreshToken}\"}}";

            FirebaseAuth? auth = await ExecuteAuthWithPostContent(request, GoogleRefreshAuth, content, SnakeCaseJsonSerializerOption);

            request.FirebaseUser.UpdateAuth(auth);

            await RefreshUserInfo(request, request.FirebaseUser);
        }

        return request.FirebaseUser.IdToken;
    }
}
