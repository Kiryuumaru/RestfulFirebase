using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Gets the fresh token of the authenticated account.
/// </summary>
public class GetFreshTokenRequest : AuthenticatedRequest
{
    /// <inheritdoc cref="GetFreshTokenRequest"/>
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

        try
        {
            if (Authorization.IsExpired())
            {
                var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{Authorization.RefreshToken}\"}}";

                FirebaseAuth? auth = await ExecuteAuthWithPostContent(content, GoogleRefreshAuth, SnakeCaseJsonSerializerOption);

                Authorization.UpdateAuth(auth);

                await RefreshUserInfo(Authorization);
            }

            return new(this, Authorization, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
