using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public interface IAuthenticatedTransactionRequest : ITransactionRequest
{
    /// <summary>
    /// Gets or sets the authenticated <see cref="Authentication.Models.FirebaseUser"/> to authenticate the request.
    /// </summary>
    public FirebaseUser? FirebaseUser { get; set; }
}
