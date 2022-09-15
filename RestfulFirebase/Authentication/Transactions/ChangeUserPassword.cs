using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

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
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or
    /// <see cref="NewPassword"/> is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(NewPassword);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"password\":\"{NewPassword}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleUpdateUser, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
