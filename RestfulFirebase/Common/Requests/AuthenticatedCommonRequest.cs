using RestfulFirebase.Authentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Common.Requests;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public class AuthenticatedCommonRequest : CommonRequest
{
    /// <summary>
    /// Gets or sets the authenticated <see cref="FirebaseUser"/> of the request.
    /// </summary>
    public FirebaseUser? FirebaseUser { get; set; }
}
