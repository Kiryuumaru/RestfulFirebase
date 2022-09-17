using System;
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

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to send password reset email to the existing account provided with the email.
/// </summary>
public class SendPasswordResetEmailRequest : AuthenticationRequest<SendPasswordResetEmailResponse>
{
    /// <summary>
    /// Gets or sets the email of the request.
    /// </summary>
    public string? Email { get; set; }

    /// <inheritdoc cref="SendPasswordResetEmailRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="SendPasswordResetEmailResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>  or
    /// <see cref="Email"/> is a null reference.
    /// </exception>
    internal override async Task<SendPasswordResetEmailResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Email);

        try
        {
            var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{Email}\"}}";

            await ExecuteWithPostContent(content, GoogleGetConfirmationCodeUrl);

            return new SendPasswordResetEmailResponse(this, null);
        }
        catch (Exception ex)
        {
            return new SendPasswordResetEmailResponse(this, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SendPasswordResetEmailRequest"/> request.
/// </summary>
public class SendPasswordResetEmailResponse : TransactionResponse<SendPasswordResetEmailRequest>
{
    internal SendPasswordResetEmailResponse(SendPasswordResetEmailRequest request, Exception? error)
        : base(request, error)
    {

    }
}
