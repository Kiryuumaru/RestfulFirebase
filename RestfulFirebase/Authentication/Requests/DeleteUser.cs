using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to delete the authenticated user.
/// </summary>
public class DeleteUserRequest : AuthenticatedRequest
{
    /// <inheritdoc cref="DeleteUserRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.Authorization"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{ \"idToken\": \"{tokenRequest.Result}\" }}";

            await ExecuteWithPostContent(content, GoogleDeleteUserUrl);

            return new(this, Authorization, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
