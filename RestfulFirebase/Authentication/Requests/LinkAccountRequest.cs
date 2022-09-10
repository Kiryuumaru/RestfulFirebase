using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to links the account with the provided email and password.
/// </summary>
public class LinkAccountRequest : AuthenticatedCommonRequest
{
    /// <summary>
    /// Gets or sets the account`s email to be linked.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the account`s password to be linked.
    /// </summary>
    public string? Password { get; set; }
}
