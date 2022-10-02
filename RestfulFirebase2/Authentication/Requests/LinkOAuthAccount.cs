using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
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

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        var providerId = GetProviderId(AuthType.Value);
        var content = $"{{\"idToken\":\"{tokenResponse.Result.IdToken}\",\"postBody\":\"access_token={OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        var refreshException = await RefreshUserInfo(Authorization);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, Authorization, null);
    }
}
