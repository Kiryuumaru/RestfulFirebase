using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Base class for all authenticated requests.
/// </summary>
public abstract class AuthenticatedRequest : AuthenticationRequest<TransactionResponse<AuthenticatedRequest, FirebaseUser>>, IAuthenticatedRequest<FirebaseUser>
{
    /// <inheritdoc/>
    public FirebaseUser? Authorization { get; set; }
}
