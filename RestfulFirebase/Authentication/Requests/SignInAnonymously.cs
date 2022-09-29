using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in anonimously.
/// </summary>
public class SignInAnonymouslyRequest : AuthenticationRequest<TransactionResponse<SignInAnonymouslyRequest, FirebaseUser>>
{
    /// <inheritdoc cref="SignInAnonymouslyRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInAnonymouslyRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);

        var content = $"{{\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleSignUpUrl, CamelCaseJsonSerializerOption);
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
