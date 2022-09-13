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
using RestfulFirebase.Authentication.Requests;
using System.Linq;
using System.Text.Json.Serialization;
using RestfulFirebase.Common.Requests;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static partial class Authentication
{
    /// <summary>
    /// Gets the reCaptcha site key to be used for sending verification code to a phone number.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the reCapcha site key.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<string> GetRecaptchaSiteKey(CommonRequest request)
    {
        RecaptchaSiteKeyDefinition? response = await ExecuteWithGet<RecaptchaSiteKeyDefinition>(request, GoogleRecaptchaParams, CamelCaseJsonSerializerOption);

        if (response == null || response.RecaptchaSiteKey == null)
        {
            throw new Exception();
        }

        return response.RecaptchaSiteKey;
    }

    /// <summary>
    /// Send a verification code to a phone number.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the sessioninfo of the verification sent.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SendVerificationCodeRequest.PhoneNumber"/> and
    /// <see cref="SendVerificationCodeRequest.RecaptchaToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<string> SendVerificationCode(SendVerificationCodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.PhoneNumber);
        ArgumentNullException.ThrowIfNull(request.RecaptchaToken);

        string content = $"{{\"phoneNumber\":\"{request.PhoneNumber}\",\"recaptchaToken\":\"{request.RecaptchaToken}\"}}";

        SessionInfoDefinition? response = await ExecuteWithPostContent<SessionInfoDefinition>(request, GoogleSendVerificationCode, content, CamelCaseJsonSerializerOption);

        if (response == null || response.SessionInfo == null)
        {
            throw new Exception();
        }

        return response.SessionInfo;
    }

    /// <summary>
    /// Creates user with the provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthEmailExistsException">
    /// The email address is already in use by another account.
    /// </exception>
    /// <exception cref="AuthWeakPasswordException">
    /// The password must be 6 characters long or more.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// Password sign-in is disabled for this project.
    /// </exception>
    /// <exception cref="AuthTooManyAttemptsException">
    /// There is an unusual activity on device.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> CreateUserWithEmailAndPassword(CreateUserWithEmailAndPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        var content = $"{{\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignUpUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        if (request.SendVerificationEmail)
        {
            await SendEmailVerification(new AuthenticatedCommonRequest()
            {
                Config = request.Config,
                FirebaseUser = user
            });
        }

        return user;
    }

    /// <summary>
    /// Sign in with provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="SignInWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthEmailNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthInvalidPasswordException">
    /// The password is invalid or the user does not have a password.
    /// </exception>
    /// <exception cref="AuthUserDisabledException">
    /// The user account has been disabled by an administrator.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithEmailAndPassword(SignInWithEmailAndPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        StringBuilder sb = new($"{{\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",");

        if (request.TenantId != null)
        {
            sb.Append($"\"tenantId\":\"{request.TenantId}\",");
        }

        sb.Append("\"returnSecureToken\":true}");

        string content = sb.ToString();

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GooglePasswordUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithPhoneNumber.SessionInfo"/> and
    /// <see cref="SignInWithPhoneNumber.Code"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public async static Task<FirebaseUser> SignInWithPhoneNumber(SignInWithPhoneNumber request)
    {
        ArgumentNullException.ThrowIfNull(request.SessionInfo);
        ArgumentNullException.ThrowIfNull(request.Code);

        string content = $"{{\"sessionInfo\":\"{request.SessionInfo}\",\"code\":\"{request.Code}\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignInWithPhoneNumber, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in with custom token provided by firebase.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SignInWithCustomTokenRequest.CustomToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidCustomTokenException">
    /// The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
    /// </exception>
    /// <exception cref="AuthCredentialMismatchException">
    /// The custom token corresponds to a different Firebase project.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithCustomToken(SignInWithCustomTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.CustomToken);

        string content = $"{{\"token\":\"{request.CustomToken}\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleCustomAuthUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in with oauth provided with auth type and oauth token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithOAuthRequest.AuthType"/> and
    /// <see cref="SignInWithOAuthRequest.OAuthToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithOAuth(SignInWithOAuthRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.AuthType);
        ArgumentNullException.ThrowIfNull(request.OAuthToken);

        var providerId = GetProviderId(request.AuthType.Value);

        string content = request.AuthType.Value switch
        {
            FirebaseAuthType.Apple => $"{{\"postBody\":\"id_token={request.OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
            _ => $"{{\"postBody\":\"access_token={request.OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
        };

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in with twitter oauth token provided with oauth access token and oauth access secret from twitter.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthAccessToken"/> and
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthTokenSecret"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithOAuthTwitterToken(SignInWithOAuthTwitterTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);
        ArgumentNullException.ThrowIfNull(request.OAuthTokenSecret);

        var providerId = GetProviderId(FirebaseAuthType.Twitter);
        var content = $"{{\"postBody\":\"access_token={request.OAuthAccessToken}&oauth_token_secret={request.OAuthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in with google id token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SignInWithGoogleIdTokenRequest.IdToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// The corresponding provider is disabled for this project.
    /// </exception>
    /// <exception cref="AuthInvalidIDPResponseException">
    /// The supplied auth credential is malformed or has expired.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithGoogleIdToken(SignInWithGoogleIdTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.IdToken);

        var providerId = GetProviderId(FirebaseAuthType.Google);
        var content = $"{{\"postBody\":\"id_token={request.IdToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Sign in anonimously.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthOperationNotAllowedException">
    /// Anonymous user sign-in is disabled for this project.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<FirebaseUser> SignInAnonymously(CommonRequest request)
    {
        var content = $"{{\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignUpUrl, content, CamelCaseJsonSerializerOption);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    /// <summary>
    /// Send password reset email to the existing account provided with the email.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SendPasswordResetEmailRequest.Email"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthEmailNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task SendPasswordResetEmail(SendPasswordResetEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Email);

        var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{request.Email}\"}}";

        await ExecuteWithPostContent(request, GoogleGetConfirmationCodeUrl, content);
    }
}
