using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class SignInWithCustomTokenRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the token provided by firebase.
    /// </summary>
    public string? CustomToken { get; set; }
}
