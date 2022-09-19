using System;
using System.Threading.Tasks;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Transactions;

/// <summary>
/// Gets the fresh token of the authenticated account.
/// </summary>
public class GetFreshTokenRequest : AuthenticatedRequest, IAuthenticatedTransactionRequest
{
    /// <inheritdoc cref="GetFreshTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or  is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            if (FirebaseUser.IsExpired())
            {
                var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{FirebaseUser.RefreshToken}\"}}";

                FirebaseAuth? auth = await ExecuteAuthWithPostContent(content, GoogleRefreshAuth, SnakeCaseJsonSerializerOption);

                FirebaseUser.UpdateAuth(auth);

                await RefreshUserInfo(FirebaseUser);
            }

            return new(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
