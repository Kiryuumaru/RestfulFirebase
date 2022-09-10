using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to creates user with the provided email and password.
/// </summary>
public class CreateUserWithEmailAndPasswordRequest : CommonRequest
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
