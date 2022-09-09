using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to change the password of the authenticated user.
/// </summary>
public class ChangeUserPasswordRequest : AuthenticatedRequest
{
    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string? NewPassword { get; set; }
}
