﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Common.Abstraction;

namespace RestfulFirebase.Authentication.Transactions;

/// <summary>
/// Gets the fresh token of the authenticated account.
/// </summary>
public class GetFreshTokenRequest : AuthenticatedRequest, IAuthenticatedTransactionRequest
{
    /// <inheritdoc cref="GetFreshTokenRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="AuthenticatedResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> or  is a null reference.
    /// </exception>
    internal override async Task<AuthenticatedResponse> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(FirebaseUser);

        try
        {
            if (FirebaseUser.IsExpired())
            {
                var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{FirebaseUser.RefreshToken}\"}}";

                FirebaseAuth? auth = await ExecuteAuthWithPostContent(content, GoogleRefreshAuth, SnakeCaseJsonSerializerOption);

                FirebaseUser.UpdateAuth(auth);

                await RefreshUserInfo(FirebaseUser);
            }

            return new AuthenticatedResponse(this, FirebaseUser, null);
        }
        catch (Exception ex)
        {
            return new AuthenticatedResponse(this, null, ex);
        }
    }
}
