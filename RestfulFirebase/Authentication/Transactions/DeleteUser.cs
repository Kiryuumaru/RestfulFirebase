using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Transactions;

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
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{ \"idToken\": \"{tokenRequest.Result}\" }}";

            await ExecuteWithPostContent(content, GoogleDeleteUserUrl);

            return new(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
