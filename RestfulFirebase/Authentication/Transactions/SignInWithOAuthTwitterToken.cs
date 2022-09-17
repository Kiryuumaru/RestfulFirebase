using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to sign in with twitter oauth token provided with oauth access token and oauth access secret from twitter.
/// </summary>
public class SignInWithOAuthTwitterTokenRequest : AuthenticationRequest<TransactionResponse<SignInWithOAuthTwitterTokenRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the access token provided by twitter.
    /// </summary>
    public string? OAuthAccessToken { get; set; }

    /// <summary>
    /// Gets or sets the oauth token secret provided by twitter
    /// </summary>
    public string? OAuthTokenSecret { get; set; }

    /// <inheritdoc cref="SignInWithOAuthTwitterTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="OAuthAccessToken"/> or
    /// <see cref="OAuthTokenSecret"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithOAuthTwitterTokenRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(OAuthAccessToken);
        ArgumentNullException.ThrowIfNull(OAuthTokenSecret);

        try
        {
            var providerId = GetProviderId(FirebaseAuthType.Twitter);
            var content = $"{{\"postBody\":\"access_token={OAuthAccessToken}&oauth_token_secret={OAuthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new(this, user, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
