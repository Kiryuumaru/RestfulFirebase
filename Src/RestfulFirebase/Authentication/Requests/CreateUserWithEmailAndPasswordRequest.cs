﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class CreateUserWithEmailAndPasswordRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> to send email verification after user creation; otherwise, <c>false</c>.
    /// </summary>
    public bool SendVerificationEmail { get; set; } = false;
}