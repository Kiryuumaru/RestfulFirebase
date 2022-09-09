using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Utilities;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Requests;
using System.Linq;
using System.Text.Json.Serialization;

namespace RestfulFirebase;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public static class FirebaseAuthentication
{
    #region Properties

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOption = new()
    {
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal const string GoogleSignInWithPhoneNumber = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={0}";
    internal const string GoogleRecaptchaParams = "https://identitytoolkit.googleapis.com/v1/recaptchaParams?key={0}";
    internal const string GoogleSendVerificationCode = "https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={0}";
    internal const string GoogleRefreshAuth = "https://securetoken.googleapis.com/v1/token?key={0}";
    internal const string GoogleCustomAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken?key={0}";
    internal const string GoogleGetUser = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={0}";
    internal const string GoogleIdentityUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyAssertion?key={0}";
    internal const string GoogleSignUpUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}";
    internal const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
    internal const string GoogleDeleteUserUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/deleteAccount?key={0}";
    internal const string GoogleGetConfirmationCodeUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key={0}";
    internal const string GoogleSetAccountUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key={0}";
    internal const string GoogleCreateAuthUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/createAuthUri?key={0}";
    internal const string GoogleUpdateUserPassword = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
    internal const string ProfileDeleteDisplayName = "DISPLAY_NAME";
    internal const string ProfileDeletePhotoUrl = "PHOTO_URL";

    #endregion

    #region Helpers

    internal static string? GetProviderId(FirebaseAuthType authType)
    {
        return authType switch
        {
            FirebaseAuthType.Facebook or
            FirebaseAuthType.Google or
            FirebaseAuthType.Apple or
            FirebaseAuthType.Github or
            FirebaseAuthType.Twitter => authType.ToEnumString(),
            FirebaseAuthType.EmailAndPassword => throw new InvalidOperationException("Email auth type cannot be used like this. Use methods specific to email & password authentication."),
            _ => throw new NotImplementedException(""),
        };
    }

    internal static async Task<string> ExecuteWithGet(AuthenticationRequest request, string googleUrl)
    {
        ArgumentNullException.ThrowIfNull(request.Config);

        HttpClient httpClient = request.HttpClient ?? new HttpClient();

        string responseData = "N/A";

        try
        {
            var response = await httpClient.GetAsync(
                new Uri(string.Format(googleUrl, request.Config.ApiKey)),
                request.CancellationToken);

            responseData = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw GetException(responseData, ex);
        }
    }

    internal static async Task<string> ExecuteWithPostContent(AuthenticationRequest request, string googleUrl, string postContent)
    {
        ArgumentNullException.ThrowIfNull(request.Config);

        HttpClient httpClient = request.HttpClient ?? new HttpClient();

        string responseData = "N/A";

        try
        {
            var response = await httpClient.PostAsync(
                new Uri(string.Format(googleUrl, request.Config.ApiKey)),
                new StringContent(postContent, Encoding.UTF8, "Application/json"),
                request.CancellationToken);

            responseData = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw GetException(responseData, ex);
        }
    }

    internal static async Task<FirebaseAuth> ExecuteAuthWithPostContent(AuthenticationRequest request, string googleUrl, string postContent)
    {
        AuthenticatedRequest? authenticatedRequest = request as AuthenticatedRequest;

        if (authenticatedRequest != null)
        {
            ArgumentNullException.ThrowIfNull(authenticatedRequest.FirebaseUser);
        }

        string responseData = await ExecuteWithPostContent(request, googleUrl, postContent);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, DefaultJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        if (authenticatedRequest != null)
        {
            authenticatedRequest.FirebaseUser!.UpdateAuth(auth);
        }

        return auth;
    }

    internal static async Task RefreshUserInfo(AuthenticationRequest request, FirebaseUser user)
    {
        var content = $"{{\"idToken\":\"{user.FirebaseToken}\"}}";

        var responseData = await ExecuteWithPostContent(request, GoogleGetUser, content);

        var resultJson = JsonDocument.Parse(responseData);
        if (!(resultJson?.RootElement.TryGetProperty("users", out JsonElement userJson) ?? false))
        {
            throw new AuthUndefinedException();
        }
        var auth = JsonSerializer.Deserialize<FirebaseAuth>(userJson.EnumerateArray().First(), DefaultJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        user.UpdateAuth(auth);
    }

    internal static Exception GetException(string responseData, Exception originalException)
    {
        string message = "";
        try
        {
            if (!string.IsNullOrEmpty(responseData) && responseData != "N/A")
            {
                //create error data template and try to parse JSON
                var errorData = new { error = new { code = 0, message = "errorid" } };
                errorData = JsonSerializerExtensions.DeserializeAnonymousType(responseData, errorData, DefaultJsonSerializerOption);

                //errorData is just null if different JSON was received
                message = errorData?.error?.message ?? "";
            }
        }
        catch (JsonException)
        {
            //the response wasn't JSON - no data to be parsed
        }
        catch (Exception ex)
        {
            return ex;
        }

        if (message.StartsWith("invalid access_token, error code 43."))
        {
            return new AuthInvalidAccessTokenException(originalException);
        }
        else if (message.StartsWith("API key not valid"))
        {
            return new AuthAPIKeyNotValidException(originalException);
        }
        else if (message.StartsWith("A system error has occurred"))
        {
            return new AuthSystemErrorException(originalException);
        }
        else if (message.StartsWith("Invalid JSON payload received"))
        {
            return new AuthInvalidJSONReceivedException(originalException);
        }
        else if (message.StartsWith("CREDENTIAL_TOO_OLD_LOGIN_AGAIN"))
        {
            return new AuthLoginCredentialsTooOldException(originalException);
        }
        else if (message.StartsWith("OPERATION_NOT_ALLOWED"))
        {
            return new AuthOperationNotAllowedException(originalException);
        }
        else if (message.StartsWith("INVALID_PROVIDER_ID"))
        {
            return new AuthInvalidProviderIDException(originalException);
        }
        else if (message.StartsWith("MISSING_REQUEST_URI"))
        {
            return new AuthMissingRequestURIException(originalException);
        }
        else if (message.StartsWith("MISSING_OR_INVALID_NONCE"))
        {
            return new AuthDuplicateCredentialUseException(originalException);
        }
        else if (message.StartsWith("INVALID_CUSTOM_TOKEN"))
        {
            return new AuthInvalidCustomTokenException(originalException);
        }
        else if (message.StartsWith("CREDENTIAL_MISMATCH"))
        {
            return new AuthCredentialMismatchException(originalException);
        }
        else if (message.StartsWith("INVALID_EMAIL"))
        {
            return new AuthInvalidEmailAddressException(originalException);
        }
        else if (message.StartsWith("MISSING_PASSWORD"))
        {
            return new AuthMissingPasswordException(originalException);
        }
        else if (message.StartsWith("EMAIL_EXISTS"))
        {
            return new AuthEmailExistsException(originalException);
        }
        else if (message.StartsWith("USER_NOT_FOUND"))
        {
            return new AuthUserNotFoundException(originalException);
        }
        else if (message.StartsWith("INVALID_PASSWORD"))
        {
            return new AuthInvalidPasswordException(originalException);
        }
        else if (message.StartsWith("EMAIL_NOT_FOUND"))
        {
            return new AuthEmailNotFoundException(originalException);
        }
        else if (message.StartsWith("USER_DISABLED"))
        {
            return new AuthUserDisabledException(originalException);
        }
        else if (message.StartsWith("MISSING_EMAIL"))
        {
            return new AuthMissingEmailException(originalException);
        }
        else if (message.StartsWith("RESET_PASSWORD_EXCEED_LIMIT"))
        {
            return new AuthResetPasswordExceedLimitException(originalException);
        }
        else if (message.StartsWith("MISSING_REQ_TYPE"))
        {
            return new AuthMissingRequestTypeException(originalException);
        }
        else if (message.StartsWith("INVALID_ID_TOKEN"))
        {
            return new AuthInvalidIDTokenException(originalException);
        }
        else if (message.StartsWith("INVALID_IDENTIFIER"))
        {
            return new AuthInvalidIdentifierException(originalException);
        }
        else if (message.StartsWith("MISSING_IDENTIFIER"))
        {
            return new AuthMissingIdentifierException(originalException);
        }
        else if (message.StartsWith("FEDERATED_USER_ID_ALREADY_LINKED"))
        {
            return new AuthAlreadyLinkedException(originalException);
        }
        else if (message.StartsWith("TOKEN_EXPIRED"))
        {
            return new AuthTokenExpiredException(originalException);
        }
        else if (message.StartsWith("INVALID_REFRESH_TOKEN"))
        {
            return new AuthInvalidRefreshTokenException(originalException);
        }
        else if (message.StartsWith("INVALID_GRANT_TYPE"))
        {
            return new AuthInvalidGrantTypeException(originalException);
        }
        else if (message.StartsWith("MISSING_REFRESH_TOKEN"))
        {
            return new AuthMissingRefreshTokenException(originalException);
        }
        else if (message.StartsWith("WEAK_PASSWORD"))
        {
            return new AuthWeakPasswordException(originalException);
        }
        else if (message.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER"))
        {
            return new AuthTooManyAttemptsException(originalException);
        }
        else if (message.StartsWith("ERROR_INVALID_CREDENTIAL"))
        {
            return new AuthStaleIDTokenException(originalException);
        }
        else if (message.StartsWith("INVALID_IDP_RESPONSE"))
        {
            return new AuthInvalidIDPResponseException(originalException);
        }
        else if (message.StartsWith("EXPIRED_OOB_CODE"))
        {
            return new AuthExpiredOOBCodeException(originalException);
        }
        else if (message.StartsWith("INVALID_OOB_CODE"))
        {
            return new AuthInvalidOOBCodeException(originalException);
        }
        else
        {
            return new AuthUndefinedException(originalException);
        }
    }

    #endregion

    #region Public Requests

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
    /// <see cref="AuthenticationRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<string> GetRecaptchaSiteKey(AuthenticationRequest request)
    {
        var responseData = await ExecuteWithGet(request, GoogleRecaptchaParams);

        var definition = new { recaptchaSiteKey = "" };

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, DefaultJsonSerializerOption);

        if (response == null)
        {
            throw new Exception();
        }

        return response.recaptchaSiteKey;
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
    /// <see cref="AuthenticationRequest.Config"/>,
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

        var responseData = await ExecuteWithPostContent(request, GoogleSendVerificationCode, content);

        var definition = new { sessionInfo = "" };

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, DefaultJsonSerializerOption);

        if (response == null)
        {
            throw new Exception();
        }

        return response.sessionInfo;
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
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="CreateUserWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<FirebaseUser> CreateUserWithEmailAndPassword(CreateUserWithEmailAndPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        var content = $"{{\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",\"returnSecureToken\":true}}";

        FirebaseAuth auth = await ExecuteAuthWithPostContent(request, GoogleSignUpUrl, content);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        if (request.SendVerificationEmail)
        {
            await SendEmailVerification(new AuthenticatedRequest()
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
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="SignInWithEmailAndPasswordRequest.Email"/> and
    /// <see cref="SignInWithEmailAndPasswordRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
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

        var auth = await ExecuteAuthWithPostContent(request, GooglePasswordUrl, sb.ToString());

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
    /// <see cref="AuthenticationRequest.Config"/>,
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

        var auth = await ExecuteAuthWithPostContent(request, GoogleSignInWithPhoneNumber, content);

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
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="SignInWithCustomTokenRequest.CustomToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithCustomToken(SignInWithCustomTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.CustomToken);

        string content = $"{{\"token\":\"{request.CustomToken}\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(request, GoogleCustomAuthUrl, content);

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
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="SignInWithOAuthRequest.AuthType"/> are either a null reference.
    /// <see cref="SignInWithOAuthRequest.OAuthToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
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

        var auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content);

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
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthAccessToken"/> are either a null reference.
    /// <see cref="SignInWithOAuthTwitterTokenRequest.OAuthTokenSecret"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithOAuthTwitterToken(SignInWithOAuthTwitterTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);
        ArgumentNullException.ThrowIfNull(request.OAuthTokenSecret);

        var providerId = GetProviderId(FirebaseAuthType.Twitter);
        var content = $"{{\"postBody\":\"access_token={request.OAuthAccessToken}&oauth_token_secret={request.OAuthTokenSecret}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content).ConfigureAwait(false);

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
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="SignInWithGoogleIdTokenRequest.IdToken"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<FirebaseUser> SignInWithGoogleIdToken(SignInWithGoogleIdTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.IdToken);

        var providerId = GetProviderId(FirebaseAuthType.Google);
        var content = $"{{\"postBody\":\"id_token={request.IdToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(request, GoogleIdentityUrl, content).ConfigureAwait(false);

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
    /// <see cref="AuthenticationRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task<FirebaseUser> SignInAnonymously(AuthenticationRequest request)
    {
        var content = $"{{\"returnSecureToken\":true}}";

        var auth = await ExecuteAuthWithPostContent(request, GoogleSignUpUrl, content).ConfigureAwait(false);

        FirebaseUser user = new(auth);

        await RefreshUserInfo(request, user);

        return user;
    }

    #endregion

    #region Authenticated Requests

    /// <summary>
    /// Send email verification to the authenticated user`s email.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task SendEmailVerification(AuthenticatedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{request.FirebaseUser.FirebaseToken}\"}}";

        await ExecuteAuthWithPostContent(request, GoogleGetConfirmationCodeUrl, content);
    }

    #endregion
}
