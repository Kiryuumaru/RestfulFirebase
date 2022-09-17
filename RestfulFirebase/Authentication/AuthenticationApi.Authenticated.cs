using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Transactions;
using System.Linq;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Abstractions;

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
    public static Task<AuthenticatedResponse> SendEmailVerification(SendEmailVerificationRequest request)
        => request.Execute();

    /// <inheritdoc cref="ChangeUserEmailRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> ChangeUserEmail(ChangeUserEmailRequest request)
        => request.Execute();

    /// <inheritdoc cref="ChangeUserPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> ChangeUserPassword(ChangeUserPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="UpdateProfileRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> UpdateProfile(UpdateProfileRequest request)
        => request.Execute();

    /// <inheritdoc cref="DeleteUserRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> DeleteUser(DeleteUserRequest request)
        => request.Execute();

    /// <inheritdoc cref="LinkAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> LinkAccount(LinkAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="LinkOAuthAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> LinkAccount(LinkOAuthAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="UnlinkAccountRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> UnlinkAccounts(UnlinkAccountRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetFreshTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> GetFreshToken(GetFreshTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetFreshTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<AuthenticatedResponse> GetFreshToken(IAuthenticatedTransactionRequest request)
        => GetFreshToken(new GetFreshTokenRequest()
        {
            CancellationToken = request.CancellationToken,
            HttpClient = request.HttpClient,
            Config = request.Config,
            FirebaseUser = request.FirebaseUser
        });
}
