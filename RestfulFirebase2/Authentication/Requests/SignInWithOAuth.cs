using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with oauth provided with auth type and oauth token.
/// </summary>
public class SignInWithOAuthRequest : AuthenticationRequest<TransactionResponse<SignInWithOAuthRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> of the oauth used.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <summary>
    /// Gets or sets the token of the provided oauth type.
    /// </summary>
    public string? OAuthToken { get; set; }

    /// <inheritdoc cref="SignInWithOAuthRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthType"/> or
    /// <see cref="OAuthToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithOAuthRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(AuthType);
        ArgumentNullException.ThrowIfNull(OAuthToken);

        var providerId = GetProviderId(AuthType.Value);
        string content = AuthType.Value switch
        {
            FirebaseAuthType.Apple => $"{{\"postBody\":\"id_token={OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
            _ => $"{{\"postBody\":\"access_token={OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
        };

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleIdentityUrl, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        FirebaseUser user = new(executeResult);

        var refreshException = await RefreshUserInfo(user);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, user, null);
    }
}