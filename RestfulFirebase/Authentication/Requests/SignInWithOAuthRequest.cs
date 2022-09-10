using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to sign in with oauth provided with auth type and oauth token.
/// </summary>
public class SignInWithOAuthRequest : CommonRequest
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
