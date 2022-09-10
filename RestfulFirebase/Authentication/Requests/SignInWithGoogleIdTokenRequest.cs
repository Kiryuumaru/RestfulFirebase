using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with google id token.
/// </summary>
public class SignInWithGoogleIdTokenRequest : CommonRequest
{
    /// <summary>
    /// Gets or sets the id token provided by google.
    /// </summary>
    public string? IdToken { get; set; }
}
