using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Common.Abstraction;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to send an email verification to the authenticated user`s email.
/// </summary>
public class SendEmailVerificationRequest : AuthenticatedRequest, IAuthenticatedTransactionRequest
{
    /// <inheritdoc cref="SendEmailVerificationRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or  is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);

            if (tokenRequest.Error != null)
            {
                throw tokenRequest.Error;
            }

            var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{tokenRequest.Result}\"}}";

            await ExecuteWithPostContent(content, GoogleGetConfirmationCodeUrl);

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
