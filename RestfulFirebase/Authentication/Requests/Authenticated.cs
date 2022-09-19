using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Base class for all authenticated requests.
/// </summary>
public abstract class AuthenticatedRequest : AuthenticationRequest<TransactionResponse<AuthenticatedRequest, FirebaseUser>>, IAuthenticatedTransactionRequest
{
    /// <inheritdoc/>
    public FirebaseUser? FirebaseUser { get; set; }
}
