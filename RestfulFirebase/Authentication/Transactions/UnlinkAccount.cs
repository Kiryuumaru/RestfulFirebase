using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

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
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or
    /// <see cref="AuthType"/> is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(AuthType);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
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
                throw new AuthUndefinedException();
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"deleteProvider\":[\"{providerId}\"]}}";

            await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
