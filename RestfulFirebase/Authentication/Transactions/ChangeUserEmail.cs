using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to change the email of the authenticated user.
/// </summary>
public class ChangeUserEmailRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the new email.
    /// </summary>
    public string? NewEmail { get; set; }

    /// <inheritdoc cref="ChangeUserEmailRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or
    /// <see cref="NewEmail"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(NewEmail);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"email\":\"{NewEmail}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleUpdateUser, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(FirebaseUser);

            return new(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}