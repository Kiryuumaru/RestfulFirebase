using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to change the password of the authenticated user.
/// </summary>
public class ChangeUserPasswordRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string? NewPassword { get; set; }

    /// <inheritdoc cref="ChangeUserPasswordRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.Authorization"/> or
    /// <see cref="NewPassword"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);
        ArgumentNullException.ThrowIfNull(NewPassword);

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        var content = $"{{\"idToken\":\"{tokenResponse.Result.IdToken}\",\"password\":\"{NewPassword}\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleUpdateUser, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        var refreshException = await RefreshUserInfo(Authorization);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, Authorization, null);
    }
}
