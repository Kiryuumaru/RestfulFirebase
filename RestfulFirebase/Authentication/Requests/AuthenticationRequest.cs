using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// The base implementation for all firebase authentication request.
/// </summary>
public class AuthenticationRequest
{
    /// <summary>
    /// Gets or sets the config of the request.
    /// </summary>
    public FirebaseConfig? Config { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="HttpClient"/> used for the request.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="CancellationToken"/> of the request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}
