using RestfulFirebase.Authentication.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class SignInWithOAuthTwitterTokenRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the access token provided by twitter.
    /// </summary>
    public string? OAuthAccessToken { get; set; }

    /// <summary>
    /// Gets or sets the oauth token secret provided by twitter
    /// </summary>
    public string? OAuthTokenSecret { get; set; }
}
