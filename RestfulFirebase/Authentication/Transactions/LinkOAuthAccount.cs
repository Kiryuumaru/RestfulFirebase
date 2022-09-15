using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

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
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/>,
    /// <see cref="AuthType"/> or
    /// <see cref="OAuthAccessToken"/> is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(AuthType);
        ArgumentNullException.ThrowIfNull(OAuthAccessToken);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var providerId = GetProviderId(AuthType.Value);
            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"postBody\":\"access_token={OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
