using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
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

        if (!Authorization.IsExpired())
        {
            return new(this, Authorization, null);
        }

        var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{Authorization.RefreshToken}\"}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleRefreshAuth, SnakeCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        Authorization.UpdateAuth(executeResult);

        var refreshException = await RefreshUserInfo(Authorization);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, Authorization, null);
    }
}
