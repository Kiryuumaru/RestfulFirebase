using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with google id token.
/// </summary>
public class SignInWithGoogleIdTokenRequest : AuthenticationRequest<TransactionResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the id token provided by google.
    /// </summary>
    public string? IdToken { get; set; }

    /// <inheritdoc cref="SignInWithGoogleIdTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="IdToken"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(IdToken);

        var providerId = GetProviderId(FirebaseAuthType.Google);
        var content = $"{{\"postBody\":\"id_token={IdToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

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
