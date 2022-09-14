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
using RestfulFirebase.Common.Responses;

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
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<AuthenticatedCommonRequest>> SendEmailVerification(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{tokenRequest.Response}\"}}";

            await ExecuteWithPostContent(request, GoogleGetConfirmationCodeUrl, content);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Change the email of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserEmailRequest.NewEmail"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<ChangeUserEmailRequest>> ChangeUserEmail(ChangeUserEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewEmail);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Response}\",\"email\":\"{request.NewEmail}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(request, GoogleUpdateUser, content, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(request, request.FirebaseUser);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Change the password of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserPasswordRequest.NewPassword"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<ChangeUserPasswordRequest>> ChangeUserPassword(ChangeUserPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewPassword);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Response}\",\"password\":\"{request.NewPassword}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(request, GoogleUpdateUser, content, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(request, request.FirebaseUser);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }

    }

    /// <summary>
    /// Update the accounts profile provided with display name and photo URL.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<UpdateProfileRequest>> UpdateProfile(UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            StringBuilder sb = new($"{{\"idToken\":\"{tokenRequest.Response}\"");
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

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Delete the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<AuthenticatedCommonRequest>> DeleteUser(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{ \"idToken\": \"{tokenRequest.Response}\" }}";

            await ExecuteWithPostContent(request, GoogleDeleteUserUrl, content);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Links the account with the provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/>,
    /// <see cref="LinkAccountRequest.Email"/> and
    /// <see cref="LinkAccountRequest.Password"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<LinkAccountRequest>> LinkAccount(LinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Response}\",\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(request, GoogleSetAccountUrl, content, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(request, request.FirebaseUser);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }


    /// <summary>
    /// Links the account with oauth provided with auth type and oauth access token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/>,
    /// <see cref="LinkOAuthAccountRequest.AuthType"/> and
    /// <see cref="LinkOAuthAccountRequest.OAuthAccessToken"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<LinkOAuthAccountRequest>> LinkAccount(LinkOAuthAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var providerId = GetProviderId(request.AuthType.Value);
            var content = $"{{\"idToken\":\"{tokenRequest.Response}\",\"postBody\":\"access_token={request.OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(request, request.FirebaseUser);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Unlinks the account with oauth provided with auth type.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> and
    /// <see cref="UnlinkAccountRequest.AuthType"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<UnlinkAccountRequest>> UnlinkAccounts(UnlinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);

        try
        {
            var tokenRequest = await GetFreshToken(request);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

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

            var content = $"{{\"idToken\":\"{tokenRequest.Response}\",\"deleteProvider\":[\"{providerId}\"]}}";

            await ExecuteAuthWithPostContent(request, GoogleSetAccountUrl, content, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(request, request.FirebaseUser);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }

    /// <summary>
    /// Gets all linked accounts of the authenticated account.
    /// </summary>
    /// <returns>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with <see cref="ProviderQueryResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<AuthenticatedCommonRequest, ProviderQueryResult>> GetLinkedAccounts(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        try
        {
            string content = $"{{\"identifier\":\"{request.FirebaseUser.Email}\", \"continueUri\": \"http://localhost\"}}";

            ProviderQueryResult? data = await ExecuteWithPostContent<ProviderQueryResult>(request, GoogleCreateAuthUrl, content, CamelCaseJsonSerializerOption);

            if (data == null)
            {
                throw new AuthUndefinedException();
            }

            data.Email = request.FirebaseUser.Email;

            return CommonResponse.Create(request, data);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<AuthenticatedCommonRequest, ProviderQueryResult>(request, null, ex);
        }
    }

    /// <summary>
    /// Gets the fresh token of the authenticated account.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the fresh token.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="AuthenticatedCommonRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<AuthenticatedCommonRequest, string>> GetFreshToken(AuthenticatedCommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        try
        {
            if (request.FirebaseUser.IsExpired())
            {
                var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{request.FirebaseUser.RefreshToken}\"}}";

                FirebaseAuth? auth = await ExecuteAuthWithPostContent(request, GoogleRefreshAuth, content, SnakeCaseJsonSerializerOption);

                request.FirebaseUser.UpdateAuth(auth);

                await RefreshUserInfo(request, request.FirebaseUser);
            }

            return CommonResponse.Create(request, request.FirebaseUser.IdToken);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<AuthenticatedCommonRequest, string>(request, null, ex);
        }
    }
}
