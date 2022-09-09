using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class SignInWithPhoneNumber : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the session info token returned from <see cref="FirebaseAuthentication.SendVerificationCode(SendVerificationCodeRequest)"/>.
    /// </summary>
    public string? SessionInfo { get; set; }

    /// <summary>
    /// Gets or sets the phone sms OTP code.
    /// </summary>
    public string? Code { get; set; }
}
