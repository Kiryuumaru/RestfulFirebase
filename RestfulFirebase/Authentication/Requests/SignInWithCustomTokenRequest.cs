using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with custom token provided by firebase.
/// </summary>
public class SignInWithCustomTokenRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the token provided by firebase.
    /// </summary>
    public string? CustomToken { get; set; }
}
