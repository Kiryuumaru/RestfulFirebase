using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to send an email verification to the authenticated user`s email.
/// </summary>
public class SendEmailVerificationRequest : AuthenticatedRequest
{
    /// <inheritdoc cref="SendEmailVerificationRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.Authorization"/> or  is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{tokenResponse.Result.IdToken}\"}}";

        var (executeResult, executeException) = await ExecuteWithPostContent(content, GoogleGetConfirmationCodeUrl);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        return new(this, Authorization, null);
    }
}
