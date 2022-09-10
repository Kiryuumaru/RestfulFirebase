using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with twitter oauth token provided with oauth access token and oauth access secret from twitter.
/// </summary>
public class SignInWithOAuthTwitterTokenRequest : CommonRequest
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
