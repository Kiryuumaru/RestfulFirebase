using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Transactions;

/// <summary>
/// Request to sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
/// </summary>
public class SignInWithPhoneNumberRequest : AuthenticationRequest<TransactionResponse<SignInWithPhoneNumberRequest, FirebaseUser>>
{
    /// <summary>
    /// Gets or sets the session info token returned from <see cref="Api.Authentication.SendVerificationCode(SendVerificationCodeRequest)"/>.
    /// </summary>
    public string? SessionInfo { get; set; }

    /// <summary>
    /// Gets or sets the phone sms OTP code.
    /// </summary>
    public string? Code { get; set; }

    /// <inheritdoc cref="SignInWithPhoneNumberRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="SessionInfo"/> or
    /// <see cref="Code"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<SignInWithPhoneNumberRequest, FirebaseUser>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(SessionInfo);
        ArgumentNullException.ThrowIfNull(Code);

        try
        {
            string content = $"{{\"sessionInfo\":\"{SessionInfo}\",\"code\":\"{Code}\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(content, GoogleSignInWithPhoneNumber, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(user);

            return new(this, user, null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }
}
