using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to link the account with oauth provided with auth type and oauth access token.
/// </summary>
public class LinkOAuthAccountRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> to be linked.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <summary>
    /// Gets or sets the token of the provided auth type to be linked.
    /// </summary>
    public string? OAuthAccessToken { get; set; }

    /// <inheritdoc cref="LinkAccountRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.Authorization"/>,
    /// <see cref="AuthType"/> or
    /// <see cref="OAuthAccessToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);
        ArgumentNullException.ThrowIfNull(AuthType);
        ArgumentNullException.ThrowIfNull(OAuthAccessToken);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var providerId = GetProviderId(AuthType.Value);
            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"postBody\":\"access_token={OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(Authorization);

            return new(this, Authorization, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
