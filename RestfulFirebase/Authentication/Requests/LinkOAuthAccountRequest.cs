using RestfulFirebase.Authentication.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to links the account with oauth provided with auth type and oauth access token.
/// </summary>
public class LinkOAuthAccountRequest : AuthenticatedCommonRequest
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> to be linked.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <summary>
    /// Gets or sets the token of the provided auth type to be linked.
    /// </summary>
    public string? OAuthAccessToken { get; set; }
}
