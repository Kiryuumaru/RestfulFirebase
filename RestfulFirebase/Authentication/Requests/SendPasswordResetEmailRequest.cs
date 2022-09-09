using RestfulFirebase.Authentication.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to send password reset email to the existing account provided with the email.
/// </summary>
public class SendPasswordResetEmailRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the email of the user to send the password reset.
    /// </summary>
    public string? Email { get; set; }
}
