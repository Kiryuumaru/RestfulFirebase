using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Request to sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
/// </summary>
public class SignInWithPhoneNumberRequest : AuthenticationRequest<SignInWithPhoneNumberResponse>
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
    /// The <see cref="Task"/> proxy that represents the <see cref="SignInWithPhoneNumberResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/>,
    /// <see cref="SessionInfo"/> or
    /// <see cref="Code"/> is a null reference.
    /// </exception>
    internal override async Task<SignInWithPhoneNumberResponse> Execute()
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

            return new SignInWithPhoneNumberResponse(this, user, null);
        }
        catch (Exception ex)
        {
            return new SignInWithPhoneNumberResponse(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="SignInWithPhoneNumberRequest"/> 
/// </summary>
public class SignInWithPhoneNumberResponse : TransactionResponse<SignInWithPhoneNumberRequest, FirebaseUser>
{
    internal SignInWithPhoneNumberResponse(SignInWithPhoneNumberRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}
