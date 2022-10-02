using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to link the account with the provided email and password.
/// </summary>
public class LinkAccountRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the account`s email to be linked.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the account`s password to be linked.
    /// </summary>
    public string? Password { get; set; }

    /// <inheritdoc cref="LinkAccountRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.Authorization"/>,
    /// <see cref="Email"/> or
    /// <see cref="Password"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);
        ArgumentNullException.ThrowIfNull(Email);
        ArgumentNullException.ThrowIfNull(Password);

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        var content = $"{{\"idToken\":\"{tokenResponse.Result.IdToken}\",\"email\":\"{Email}\",\"password\":\"{Password}\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);
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
