using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to send a verification code to a phone number.
/// </summary>
public class SendVerificationCodeRequest : CommonRequest
{
    /// <summary>
    /// The phone number to send verification code.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The recaptcha token from Google reCaptcha.
    /// </summary>
    public string? RecaptchaToken { get; set; }
}
