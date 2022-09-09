using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class SignInWithGoogleIdTokenRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the id token provided by google.
    /// </summary>
    public string? IdToken { get; set; }
}
