using RestfulFirebase.Common.Internals;
using System;
using System.Threading.Tasks;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Transactions;

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

        try
        {
            var content = $"{{\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleSignUpUrl, CamelCaseJsonSerializerOption);

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
