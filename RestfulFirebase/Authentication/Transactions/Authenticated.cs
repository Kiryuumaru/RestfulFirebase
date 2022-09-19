using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Exceptions;
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
