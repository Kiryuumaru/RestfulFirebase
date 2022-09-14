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
using RestfulFirebase.Common.Responses;
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
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with reCapcha site key.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> is a null reference.
    /// </exception>
    public static async Task<CommonResponse<CommonRequest, string>> GetRecaptchaSiteKey(CommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);

        try
        {
            RecaptchaSiteKeyDefinition? response = await ExecuteWithGet<RecaptchaSiteKeyDefinition>(request, GoogleRecaptchaParams, CamelCaseJsonSerializerOption);

            if (response == null || response.RecaptchaSiteKey == null)
            {
                throw new AuthUndefinedException();
            }

            return CommonResponse.Create(request, response.RecaptchaSiteKey);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<CommonRequest, string>(request, null, ex);
        }
    }

    /// <summary>
    /// Send a verification code to a phone number.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with sessioninfo of the verification sent.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SendVerificationCodeRequest.PhoneNumber"/> and
    /// <see cref="SendVerificationCodeRequest.RecaptchaToken"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SendVerificationCodeRequest, string>> SendVerificationCode(SendVerificationCodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.PhoneNumber);
        ArgumentNullException.ThrowIfNull(request.RecaptchaToken);

        try
        {
            string content = $"{{\"phoneNumber\":\"{request.PhoneNumber}\",\"recaptchaToken\":\"{request.RecaptchaToken}\"}}";

            SessionInfoDefinition? response = await ExecuteWithPostContent<SessionInfoDefinition>(request, GoogleSendVerificationCode, content, CamelCaseJsonSerializerOption);

            if (response == null || response.SessionInfo == null)
            {
                throw new Exception();
            }

            return CommonResponse.Create(request, response.SessionInfo);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SendVerificationCodeRequest, string>(request, null, ex);
        }
    }

    /// <summary>
    /// Creates user with the provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<CreateUserWithEmailAndPasswordRequest, FirebaseUser>> CreateUserWithEmailAndPassword(CreateUserWithEmailAndPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        try
        {
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

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<CreateUserWithEmailAndPasswordRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in with provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="SignInWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SignInWithEmailAndPasswordRequest, FirebaseUser>> SignInWithEmailAndPassword(SignInWithEmailAndPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        try
        {
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

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithEmailAndPasswordRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in a phone number with the provided sessionInfo and code from reCaptcha validation and sms OTP message.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithPhoneNumber.SessionInfo"/> and
    /// <see cref="SignInWithPhoneNumber.Code"/> are either a null reference.
    /// </exception>
    public async static Task<CommonResponse<SignInWithPhoneNumber, FirebaseUser>> SignInWithPhoneNumber(SignInWithPhoneNumber request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.SessionInfo);
        ArgumentNullException.ThrowIfNull(request.Code);

        try
        {
            string content = $"{{\"sessionInfo\":\"{request.SessionInfo}\",\"code\":\"{request.Code}\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignInWithPhoneNumber, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithPhoneNumber, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in with custom token provided by firebase.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SignInWithCustomTokenRequest.CustomToken"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SignInWithCustomTokenRequest, FirebaseUser>> SignInWithCustomToken(SignInWithCustomTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.CustomToken);

        try
        {
            string content = $"{{\"token\":\"{request.CustomToken}\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleCustomAuthUrl, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithCustomTokenRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in with oauth provided with auth type and oauth token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithOAuthRequest.AuthType"/> and
    /// <see cref="SignInWithOAuthRequest.OAuthToken"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SignInWithOAuthRequest, FirebaseUser>> SignInWithOAuth(SignInWithOAuthRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.AuthType);
        ArgumentNullException.ThrowIfNull(request.OAuthToken);

        try
        {
            var providerId = GetProviderId(request.AuthType.Value);

            string content = request.AuthType.Value switch
            {
                FirebaseAuthType.Apple => $"{{\"postBody\":\"id_token={request.OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
                _ => $"{{\"postBody\":\"access_token={request.OAuthToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}",
            };

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithOAuthRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in with twitter oauth token provided with oauth access token and oauth access secret from twitter.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthAccessToken"/> and
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthTokenSecret"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SignInWithOAuthTwitterTokenRequest, FirebaseUser>> SignInWithOAuthTwitterToken(SignInWithOAuthTwitterTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);
        ArgumentNullException.ThrowIfNull(request.OAuthTokenSecret);

        try
        {
            var providerId = GetProviderId(FirebaseAuthType.Twitter);
            var content = $"{{\"postBody\":\"access_token={request.OAuthAccessToken}&oauth_token_secret={request.OAuthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithOAuthTwitterTokenRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in with google id token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SignInWithGoogleIdTokenRequest.IdToken"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SignInWithGoogleIdTokenRequest, FirebaseUser>> SignInWithGoogleIdToken(SignInWithGoogleIdTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.IdToken);

        try
        {
            var providerId = GetProviderId(FirebaseAuthType.Google);
            var content = $"{{\"postBody\":\"id_token={request.IdToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<SignInWithGoogleIdTokenRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Sign in anonimously.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/> with the authenticated <see cref="FirebaseUser"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> is a null reference.
    /// </exception>
    public static async Task<CommonResponse<CommonRequest, FirebaseUser>> SignInAnonymously(CommonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);

        try
        {
            var content = $"{{\"returnSecureToken\":true}}";

            FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignUpUrl, content, CamelCaseJsonSerializerOption);

            FirebaseUser user = new(auth);

            await RefreshUserInfo(request, user);

            return CommonResponse.Create(request, user);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create<CommonRequest, FirebaseUser>(request, null, ex);
        }
    }

    /// <summary>
    /// Send password reset email to the existing account provided with the email.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="CommonResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="SendPasswordResetEmailRequest.Email"/> are either a null reference.
    /// </exception>
    public static async Task<CommonResponse<SendPasswordResetEmailRequest>> SendPasswordResetEmail(SendPasswordResetEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Email);

        try
        {
            var content = $"{{\"requestType\":\"PASSWORD_RESET\",\"email\":\"{request.Email}\"}}";

            await ExecuteWithPostContent(request, GoogleGetConfirmationCodeUrl, content);

            return CommonResponse.Create(request);
        }
        catch (Exception ex)
        {
            return CommonResponse.Create(request, ex);
        }
    }
}
