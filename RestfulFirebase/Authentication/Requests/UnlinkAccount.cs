using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Common.Utilities;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to unlink the account with oauth provided with auth type.
/// </summary>
public class UnlinkAccountRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> to unlink.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <inheritdoc cref="UnlinkAccountRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.Authorization"/> or
    /// <see cref="AuthType"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Authorization);
        ArgumentNullException.ThrowIfNull(AuthType);

        var tokenResponse = await Api.Authentication.GetFreshToken(this);
        if (tokenResponse.Result == null)
        {
            return new(this, null, tokenResponse.Error);
        }

        string? providerId;
        if (AuthType.Value == FirebaseAuthType.EmailAndPassword)
        {
            providerId = AuthType.Value.ToEnumString();
        }
        else
        {
            providerId = GetProviderId(AuthType.Value);
        }

        if (string.IsNullOrEmpty(providerId))
        {
            throw new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default);
        }

        var content = $"{{\"idToken\":\"{tokenResponse.Result.IdToken}\",\"deleteProvider\":[\"{providerId}\"]}}";

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
