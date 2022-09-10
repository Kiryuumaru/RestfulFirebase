using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to change the email of the authenticated user.
/// </summary>
public class ChangeUserEmailRequest : AuthenticatedCommonRequest
{
    /// <summary>
    /// Gets or sets the new email.
    /// </summary>
    public string? NewEmail { get; set; }
}
