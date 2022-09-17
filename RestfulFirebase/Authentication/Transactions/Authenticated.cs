﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Base class for all authenticated requests.
/// </summary>
public abstract class AuthenticatedRequest : AuthenticationRequest<AuthenticatedResponse>, IAuthenticatedTransactionRequest
{
    /// <inheritdoc/>
    public FirebaseUser? FirebaseUser { get; set; }
}

/// <summary>
/// The response of the <see cref="AuthenticatedRequest"/> request.
/// </summary>
public class AuthenticatedResponse : TransactionResponse<AuthenticatedRequest, FirebaseUser>
{
    internal AuthenticatedResponse(AuthenticatedRequest request, FirebaseUser? result, Exception? error)
        : base(request, result, error)
    {

    }
}
