using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Enums;
using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Transactions;
using System.Linq;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Transactions;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static partial class Authentication
{
    /// <inheritdoc cref="SendEmailVerificationRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> SendEmailVerification(SendEmailVerificationRequest request)
        => request.Execute();

    /// <inheritdoc cref="ChangeUserEmailRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> ChangeUserEmail(ChangeUserEmailRequest request)
        => request.Execute();

    /// <inheritdoc cref="ChangeUserPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> ChangeUserPassword(ChangeUserPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="UpdateProfileRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> UpdateProfile(UpdateProfileRequest request)
        => request.Execute();

    /// <inheritdoc cref="DeleteUserRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> DeleteUser(DeleteUserRequest request)
        => request.Execute();

    /// <inheritdoc cref="LinkAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> LinkAccount(LinkAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="LinkOAuthAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> LinkAccount(LinkOAuthAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="UnlinkAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> UnlinkAccounts(UnlinkAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetFreshTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> GetFreshToken(GetFreshTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetFreshTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<AuthenticatedRequest, FirebaseUser>> GetFreshToken(IAuthenticatedTransactionRequest request)
        => GetFreshToken(new GetFreshTokenRequest()
        {
            CancellationToken = request.CancellationToken,
            HttpClient = request.HttpClient,
            Config = request.Config,
            FirebaseUser = request.FirebaseUser
        });
}
