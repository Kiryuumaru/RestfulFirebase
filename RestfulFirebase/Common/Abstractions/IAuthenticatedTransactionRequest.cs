using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public interface IAuthenticatedTransactionRequest : ITransactionRequest
{
    /// <summary>
    /// Gets or sets the authenticated <see cref="Common.Models.FirebaseUser"/> to authenticate the request.
    /// </summary>
    public FirebaseUser? FirebaseUser { get; set; }
}
