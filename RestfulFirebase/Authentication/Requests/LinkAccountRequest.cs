﻿using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
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

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{\"idToken\":\"{tokenRequest.Result}\",\"email\":\"{Email}\",\"password\":\"{Password}\",\"returnSecureToken\":true}}";

            await ExecuteAuthWithPostContent(content, GoogleSetAccountUrl, CamelCaseJsonSerializerOption);

            await RefreshUserInfo(Authorization);

            return new(this, Authorization, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
