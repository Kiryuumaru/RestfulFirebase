﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to send an email verification to the authenticated user`s email.
/// </summary>
public class SendEmailVerificationRequest : AuthenticatedRequest, IAuthenticatedTransactionRequest
{
    /// <inheritdoc cref="SendEmailVerificationRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or  is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            tokenRequest.ThrowIfErrorOrEmptyResult();

            var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{tokenRequest.Result}\"}}";

            await ExecuteWithPostContent(content, GoogleGetConfirmationCodeUrl);

            return new(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}