using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Transactions;

/// <summary>
/// Base class for all authenticated requests.
/// </summary>
public abstract class AuthenticatedRequest : AuthenticationRequest<TransactionResponse<AuthenticatedRequest, FirebaseUser>>, IAuthenticatedTransactionRequest
{
    /// <inheritdoc/>
    public FirebaseUser? FirebaseUser { get; set; }
}
