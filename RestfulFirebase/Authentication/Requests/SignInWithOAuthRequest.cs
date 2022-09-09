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
public class SignInWithOAuthRequest : AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> of the oauth used.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }

    /// <summary>
    /// Gets or sets the token of the provided oauth type.
    /// </summary>
    public string? OAuthToken { get; set; }
}
