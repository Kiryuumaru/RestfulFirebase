using RestfulFirebase.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Abstraction;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public interface IAuthenticatedTransactionRequest : ITransactionRequest
{
    /// <summary>
    /// Gets or sets the authenticated <see cref="Authentication.FirebaseUser"/> to authenticate the request.
    /// </summary>
    public FirebaseUser? FirebaseUser { get; set; }
}
