using System.Threading.Tasks;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.Common.Requests;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static partial class Authentication
{
    /// <inheritdoc cref="GetRecaptchaSiteKeyRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<GetRecaptchaSiteKeyRequest, string>> GetRecaptchaSiteKey(GetRecaptchaSiteKeyRequest request)
        => request.Execute();

    /// <inheritdoc cref="SendVerificationCodeRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SendVerificationCodeRequest, string>> SendVerificationCode(SendVerificationCodeRequest request)
        => request.Execute();

    /// <inheritdoc cref="CreateUserWithEmailAndPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<CreateUserWithEmailAndPasswordRequest, FirebaseUser>> CreateUserWithEmailAndPassword(CreateUserWithEmailAndPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithEmailAndPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithEmailAndPasswordRequest, FirebaseUser>> SignInWithEmailAndPassword(SignInWithEmailAndPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithPhoneNumberRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithPhoneNumberRequest, FirebaseUser>> SignInWithPhoneNumber(SignInWithPhoneNumberRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithCustomTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithCustomTokenRequest, FirebaseUser>> SignInWithCustomToken(SignInWithCustomTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithOAuthRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithOAuthRequest, FirebaseUser>> SignInWithOAuth(SignInWithOAuthRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithOAuthTwitterTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithOAuthTwitterTokenRequest, FirebaseUser>> SignInWithOAuthTwitterToken(SignInWithOAuthTwitterTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithGoogleIdTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>> SignInWithGoogleIdToken(SignInWithGoogleIdTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInAnonymouslyRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SignInAnonymouslyRequest, FirebaseUser>> SignInAnonymously(SignInAnonymouslyRequest request)
        => request.Execute();

    /// <inheritdoc cref="SendPasswordResetEmailRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<SendPasswordResetEmailRequest>> SendPasswordResetEmail(SendPasswordResetEmailRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetLinkedAccountsRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<TransactionResponse<GetLinkedAccountsRequest, ProviderQueryResult>> GetLinkedAccounts(GetLinkedAccountsRequest request)
        => request.Execute();
}
