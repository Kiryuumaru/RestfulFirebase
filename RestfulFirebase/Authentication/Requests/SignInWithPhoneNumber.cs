using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Requests;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Authentication.Requests;

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

        string content = $"{{\"sessionInfo\":\"{SessionInfo}\",\"code\":\"{Code}\",\"returnSecureToken\":true}}";

        var (executeResult, executeException) = await ExecuteAuthWithPostContent(content, GoogleSignInWithPhoneNumber, CamelCaseJsonSerializerOption);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        FirebaseUser user = new(executeResult);

        var refreshException = await RefreshUserInfo(user);
        if (refreshException != null)
        {
            return new(this, null, refreshException);
        }

        return new(this, user, null);
    }
}
