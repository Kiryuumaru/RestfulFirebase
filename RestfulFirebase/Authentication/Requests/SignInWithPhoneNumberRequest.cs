﻿using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
/// </summary>
public class SignInWithPhoneNumber : CommonRequest
{
    /// <summary>
    /// Gets or sets the session info token returned from <see cref="Api.Authentication.SendVerificationCode(SendVerificationCodeRequest)"/>.
    /// </summary>
    public string? SessionInfo { get; set; }

    /// <summary>
    /// Gets or sets the phone sms OTP code.
    /// </summary>
    public string? Code { get; set; }
}