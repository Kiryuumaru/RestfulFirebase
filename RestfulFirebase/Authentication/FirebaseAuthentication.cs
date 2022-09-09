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

    internal static readonly JsonSerializerOptions SnakeCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal static readonly JsonSerializerOptions CamelCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
    internal const string GoogleUpdateUser = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";
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

    internal static async Task RefreshUserInfo(AuthenticationRequest request, FirebaseUser user)
    {
        var content = $"{{\"idToken\":\"{user.IdToken}\"}}";

        var responseData = await ExecuteWithPostContent(request, GoogleGetUser, content);

        var resultJson = JsonDocument.Parse(responseData);
        if (!(resultJson?.RootElement.TryGetProperty("users", out JsonElement userJson) ?? false))
        {
            throw new AuthUndefinedException();
        }
        var auth = JsonSerializer.Deserialize<FirebaseAuth>(userJson.EnumerateArray().First(), CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        user.UpdateAuth(auth);
        user.UpdateInfo(auth);
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
                errorData = JsonSerializerExtensions.DeserializeAnonymousType(responseData, errorData, CamelCaseJsonSerializerOption);

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

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, CamelCaseJsonSerializerOption);

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

        var response = JsonSerializerExtensions.DeserializeAnonymousType(responseData, definition, CamelCaseJsonSerializerOption);

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

        var responseData = await ExecuteWithPostContent(request, GoogleSignUpUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GooglePasswordUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GoogleSignInWithPhoneNumber, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GoogleCustomAuthUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GoogleIdentityUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GoogleIdentityUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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

        var responseData = await ExecuteWithPostContent(request, GoogleIdentityUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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
    public static async Task<FirebaseUser> SignInAnonymously(AuthenticationRequest request)
    {
        var content = $"{{\"returnSecureToken\":true}}";

        var responseData = await ExecuteWithPostContent(request, GoogleSignUpUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

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
    /// <see cref="AuthenticationRequest.Config"/> and
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
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task SendEmailVerification(AuthenticatedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        var content = $"{{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"{token}\"}}";

        await ExecuteWithPostContent(request, GoogleGetConfirmationCodeUrl, content);
    }

    /// <summary>
    /// Change the email of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserEmailRequest.NewEmail"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthEmailExistsException">
    /// The email address is already in use by another account.
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
    public static async Task ChangeUserEmail(ChangeUserEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewEmail);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"email\":\"{request.NewEmail}\",\"returnSecureToken\":true}}";

        var responseData = await ExecuteWithPostContent(request, GoogleUpdateUser, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Change the password of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> and
    /// <see cref="ChangeUserPasswordRequest.NewPassword"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthWeakPasswordException">
    /// The password must be 6 characters long or more.
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
    public static async Task ChangeUserPassword(ChangeUserPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.NewPassword);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"password\":\"{request.NewPassword}\",\"returnSecureToken\":true}}";

        var responseData = await ExecuteWithPostContent(request, GoogleUpdateUser, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Update the accounts profile provided with display name and photo URL.
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
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task UpdateProfile(UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        StringBuilder sb = new($"{{\"idToken\":\"{token}\"");
        if (!string.IsNullOrWhiteSpace(request.DisplayName) && !string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            sb.Append($",\"displayName\":\"{request.DisplayName}\",\"photoUrl\":\"{request.PhotoUrl}\"");
        }
        else if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            sb.Append($",\"displayName\":\"{request.DisplayName}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeletePhotoUrl}\"]");
        }
        else if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            sb.Append($",\"photoUrl\":\"{request.PhotoUrl}\"");
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\"]");
        }
        else
        {
            sb.Append($",\"deleteAttribute\":[\"{ProfileDeleteDisplayName}\",\"{ProfileDeletePhotoUrl}\"]");
        }

        sb.Append($",\"returnSecureToken\":true}}");

        string content = sb.ToString();

        var responseData = await ExecuteWithPostContent(request, GoogleSetAccountUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Delete the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// There is no user record corresponding to this identifier. The user may have been deleted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    public static async Task DeleteUser(AuthenticatedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string token = await GetFreshToken(request);

        var content = $"{{ \"idToken\": \"{token}\" }}";

        await ExecuteWithPostContent(request, GoogleDeleteUserUrl, content);
    }

    /// <summary>
    /// Links the account with the provided email and password.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/>,
    /// <see cref="LinkAccountRequest.Email"/> and
    /// <see cref="LinkAccountRequest.Password"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthLoginCredentialsTooOldException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthTokenExpiredException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthWeakPasswordException">
    /// The password must be 6 characters long or more.
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
    public static async Task LinkAccount(LinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.Email);
        ArgumentNullException.ThrowIfNull(request.Password);

        string token = await GetFreshToken(request);

        var content = $"{{\"idToken\":\"{token}\",\"email\":\"{request.Email}\",\"password\":\"{request.Password}\",\"returnSecureToken\":true}}";

        var responseData = await ExecuteWithPostContent(request, GoogleSetAccountUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }


    /// <summary>
    /// Links the account with oauth provided with auth type and oauth access token.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/>,
    /// <see cref="LinkOAuthAccountRequest.AuthType"/> and
    /// <see cref="LinkOAuthAccountRequest.OAuthAccessToken"/> are either a null reference.
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
    /// <exception cref="AuthEmailExistsException">
    /// The email address is already in use by another account.
    /// </exception>
    /// <exception cref="AuthAlreadyLinkedException">
    /// This credential is already associated with a different user account.
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
    public static async Task LinkAccount(LinkOAuthAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);
        ArgumentNullException.ThrowIfNull(request.OAuthAccessToken);

        string token = await GetFreshToken(request);

        var providerId = GetProviderId(request.AuthType.Value);
        var content = $"{{\"idToken\":\"{token}\",\"postBody\":\"access_token={request.OAuthAccessToken}&providerId={providerId}\",\"requestUri\":\"http://localhost\",\"returnSecureToken\":true}}";

        var responseData = await ExecuteWithPostContent(request, GoogleIdentityUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Unlinks the account with oauth provided with auth type.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/>,
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> and
    /// <see cref="UnlinkAccountRequest.AuthType"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
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
    public static async Task UnlinkAccounts(UnlinkAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);
        ArgumentNullException.ThrowIfNull(request.AuthType);

        string token = await GetFreshToken(request);

        string? providerId;
        if (request.AuthType.Value == FirebaseAuthType.EmailAndPassword)
        {
            providerId = request.AuthType.Value.ToEnumString();
        }
        else
        {
            providerId = GetProviderId(request.AuthType.Value);
        }

        if (string.IsNullOrEmpty(providerId))
        {
            throw new AuthUndefinedException();
        }

        var content = $"{{\"idToken\":\"{token}\",\"deleteProvider\":[\"{providerId}\"]}}";

        var responseData = await ExecuteWithPostContent(request, GoogleSetAccountUrl, content);

        FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        await RefreshUserInfo(request, request.FirebaseUser);
    }

    /// <summary>
    /// Gets all linked accounts of the authenticated account.
    /// </summary>
    /// <returns>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// The <see cref="Task"/>{<see cref="ProviderQueryResult"/>} proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthInvalidEmailAddressException">
    /// The email address is badly formatted.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<ProviderQueryResult> GetLinkedAccounts(AuthenticatedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        string content = $"{{\"identifier\":\"{request.FirebaseUser.Email}\", \"continueUri\": \"http://localhost\"}}";

        var responseData = await ExecuteWithPostContent(request, GoogleCreateAuthUrl, content);

        ProviderQueryResult? data = JsonSerializer.Deserialize<ProviderQueryResult>(responseData, CamelCaseJsonSerializerOption);


        if (data == null)
        {
            throw new AuthUndefinedException();
        }

        data.Email = request.FirebaseUser.Email;

        return data;
    }

    /// <summary>
    /// Gets the fresh token of the authenticated account.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy of the specified task.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="AuthenticationRequest.Config"/> and
    /// <see cref="AuthenticatedRequest.FirebaseUser"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthTokenExpiredException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserDisabledException">
    /// The user account has been disabled by an administrator.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthInvalidRefreshTokenException">
    /// An invalid refresh token is provided.
    /// </exception>
    /// <exception cref="AuthInvalidJSONReceivedException">
    /// Invalid JSON payload received.
    /// </exception>
    /// <exception cref="AuthMissingRefreshTokenException">
    /// No refresh token provided.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    public static async Task<string> GetFreshToken(AuthenticatedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.FirebaseUser);

        if (request.FirebaseUser.IsExpired())
        {
            var content = $"{{\"grant_type\":\"refresh_token\", \"refresh_token\":\"{request.FirebaseUser.RefreshToken}\"}}";

            var responseData = await ExecuteWithPostContent(request, GoogleRefreshAuth, content);

            FirebaseAuth? auth = JsonSerializer.Deserialize<FirebaseAuth>(responseData, SnakeCaseJsonSerializerOption);

            if (auth == null)
            {
                throw new AuthUndefinedException();
            }

            request.FirebaseUser.UpdateAuth(auth);

            await RefreshUserInfo(request, request.FirebaseUser);
        }

        return request.FirebaseUser.IdToken;
    }

    #endregion
}
