using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class SendVerificationCodeRequest : AuthenticationRequest
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
