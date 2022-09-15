using System.Threading.Tasks;
using RestfulFirebase.Authentication.Transactions;

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
    public static Task<GetRecaptchaSiteKeyResponse> GetRecaptchaSiteKey(GetRecaptchaSiteKeyRequest request)
        => request.Execute();

    /// <inheritdoc cref="SendVerificationCodeRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SendVerificationCodeResponse> SendVerificationCode(SendVerificationCodeRequest request)
        => request.Execute();

    /// <inheritdoc cref="CreateUserWithEmailAndPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<CreateUserWithEmailAndPasswordResponse> CreateUserWithEmailAndPassword(CreateUserWithEmailAndPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithEmailAndPasswordRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithEmailAndPasswordResponse> SignInWithEmailAndPassword(SignInWithEmailAndPasswordRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithPhoneNumberRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithPhoneNumberResponse> SignInWithPhoneNumber(SignInWithPhoneNumberRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithCustomTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithCustomTokenResponse> SignInWithCustomToken(SignInWithCustomTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithOAuthRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithOAuthResponse> SignInWithOAuth(SignInWithOAuthRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithOAuthTwitterTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithOAuthTwitterTokenResponse> SignInWithOAuthTwitterToken(SignInWithOAuthTwitterTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInWithGoogleIdTokenRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInWithGoogleIdTokenResponse> SignInWithGoogleIdToken(SignInWithGoogleIdTokenRequest request)
        => request.Execute();

    /// <inheritdoc cref="SignInAnonymouslyRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SignInAnonymouslyResponse> SignInAnonymously(SignInAnonymouslyRequest request)
        => request.Execute();

    /// <inheritdoc cref="SendPasswordResetEmailRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<SendPasswordResetEmailResponse> SendPasswordResetEmail(SendPasswordResetEmailRequest request)
        => request.Execute();

    /// <inheritdoc cref="GetLinkedAccountsRequest.Execute"/>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    public static Task<GetLinkedAccountsResponse> GetLinkedAccounts(GetLinkedAccountsRequest request)
        => request.Execute();
}
