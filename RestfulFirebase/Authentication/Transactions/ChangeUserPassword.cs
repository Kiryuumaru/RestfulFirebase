﻿using RestfulFirebase.Common.Models;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Transactions;

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
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or
    /// <see cref="NewPassword"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);
        ArgumentNullException.ThrowIfNull(NewPassword);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"password\":\"{NewPassword}\",\"returnSecureToken\":true}}";

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
