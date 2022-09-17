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
/// Request to send a verification code to a phone number.
/// </summary>
public class SendVerificationCodeRequest : AuthenticationRequest<SendVerificationCodeResponse>
{
    /// <summary>
    /// The phone number to send verification code.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The recaptcha token from Google reCaptcha.
    /// </summary>
    public string? RecaptchaToken { get; set; }

    /// <inheritdoc cref="SendVerificationCodeRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="SendVerificationCodeResponse"/> with the session info <see cref="string"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="PhoneNumber"/> or
    /// <see cref="RecaptchaToken"/> is a null reference.
    /// </exception>
    internal override async Task<SendVerificationCodeResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(PhoneNumber);
        ArgumentNullException.ThrowIfNull(RecaptchaToken);

        try
        {
            string content = $"{{\"phoneNumber\":\"{PhoneNumber}\",\"recaptchaToken\":\"{RecaptchaToken}\"}}";

            SessionInfoDefinition? response = await ExecuteWithPostContent<SessionInfoDefinition>(content, GoogleSendVerificationCode, CamelCaseJsonSerializerOption);

            if (response == null || response.SessionInfo == null)
            {
                throw new Exception();
            }

            return new SendVerificationCodeResponse(this, response.SessionInfo, null);
        }
        catch (Exception ex)
        {
            return new SendVerificationCodeResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SendVerificationCodeRequest"/> request.
/// </summary>
public class SendVerificationCodeResponse : TransactionResponse<SendVerificationCodeRequest, string>
{
    internal SendVerificationCodeResponse(SendVerificationCodeRequest request, string? result, Exception? error)
        : base(request, result, error)
    {

    }
}
