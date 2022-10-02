using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.Common.Internals;
using System.Linq;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Authentication.Exceptions;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Base class for all authentication requests.
/// </summary>
public abstract class AuthenticationRequest<TResponse> : TransactionRequest<TResponse>
    where TResponse : TransactionResponse
{
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

    internal virtual string BuildUrl(string googleUrl)
    {
        ArgumentNullException.ThrowIfNull(Config);

        return string.Format(googleUrl, Config.ApiKey);
    }

    internal override Task<HttpClient> GetClient()
    {
        return Task.FromResult(HttpClient ?? new HttpClient());
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ErrorData))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal override async Task<Exception> GetHttpException(HttpRequestMessage? request, HttpResponseMessage? response, HttpStatusCode httpStatusCode, Exception exception)
    {
        string? requestUrlStr = null;
        string? requestContentStr = null;
        string? responseStr = null;
        if (request != null)
        {
            if (request.RequestUri != null)
            {
                requestUrlStr = request.RequestUri.ToString();
            }
            if (request.Content != null)
            {
                requestContentStr = await request.Content.ReadAsStringAsync();
            }
        }
        if (response != null)
        {
            responseStr = await response.Content.ReadAsStringAsync();
        }

        string? message = null;
        try
        {
            if (responseStr != null && !string.IsNullOrEmpty(responseStr) && responseStr != "N/A")
            {
                ErrorData? errorData = JsonSerializer.Deserialize<ErrorData>(responseStr, CamelCaseJsonSerializerOption);
                message = errorData?.Error?.Message ?? "";
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

        AuthErrorType errorType;

        if (message == null)
        {
            errorType = AuthErrorType.UndefinedException;
        }
        else if (message.StartsWith("FEDERATED_USER_ID_ALREADY_LINKED"))
        {
            errorType = AuthErrorType.AlreadyLinkedException;
        }
        else if (message.StartsWith("API key not valid"))
        {
            errorType = AuthErrorType.APIKeyNotValidException;
        }
        else if (message.StartsWith("CREDENTIAL_MISMATCH"))
        {
            errorType = AuthErrorType.CredentialMismatchException;
        }
        else if (message.StartsWith("MISSING_OR_INVALID_NONCE"))
        {
            errorType = AuthErrorType.DuplicateCredentialUseException;
        }
        else if (message.StartsWith("EMAIL_EXISTS"))
        {
            errorType = AuthErrorType.EmailExistsException;
        }
        else if (message.StartsWith("EMAIL_NOT_FOUND"))
        {
            errorType = AuthErrorType.EmailNotFoundException;
        }
        else if (message.StartsWith("EXPIRED_OOB_CODE"))
        {
            errorType = AuthErrorType.ExpiredOOBCodeException;
        }
        else if (message.StartsWith("invalid access_token, error code 43."))
        {
            errorType = AuthErrorType.InvalidAccessTokenException;
        }
        else if (message.StartsWith("INVALID_CUSTOM_TOKEN"))
        {
            errorType = AuthErrorType.InvalidCustomTokenException;
        }
        else if (message.StartsWith("INVALID_EMAIL"))
        {
            errorType = AuthErrorType.InvalidEmailAddressException;
        }
        else if (message.StartsWith("INVALID_GRANT_TYPE"))
        {
            errorType = AuthErrorType.InvalidGrantTypeException;
        }
        else if (message.StartsWith("INVALID_IDENTIFIER"))
        {
            errorType = AuthErrorType.InvalidIdentifierException;
        }
        else if (message.StartsWith("INVALID_IDP_RESPONSE"))
        {
            errorType = AuthErrorType.InvalidIDPResponseException;
        }
        else if (message.StartsWith("INVALID_ID_TOKEN"))
        {
            errorType = AuthErrorType.InvalidIDTokenException;
        }
        else if (message.StartsWith("Invalid JSON payload received"))
        {
            errorType = AuthErrorType.InvalidJSONReceivedException;
        }
        else if (message.StartsWith("INVALID_OOB_CODE"))
        {
            errorType = AuthErrorType.InvalidOOBCodeException;
        }
        else if (message.StartsWith("INVALID_PASSWORD"))
        {
            errorType = AuthErrorType.InvalidPasswordException;
        }
        else if (message.StartsWith("INVALID_PROVIDER_ID"))
        {
            errorType = AuthErrorType.InvalidProviderIDException;
        }
        else if (message.StartsWith("INVALID_REFRESH_TOKEN"))
        {
            errorType = AuthErrorType.InvalidRefreshTokenException;
        }
        else if (message.StartsWith("CREDENTIAL_TOO_OLD_LOGIN_AGAIN"))
        {
            errorType = AuthErrorType.LoginCredentialsTooOldException;
        }
        else if (message.StartsWith("MISSING_EMAIL"))
        {
            errorType = AuthErrorType.MissingEmailException;
        }
        else if (message.StartsWith("MISSING_IDENTIFIER"))
        {
            errorType = AuthErrorType.MissingIdentifierException;
        }
        else if (message.StartsWith("MISSING_PASSWORD"))
        {
            errorType = AuthErrorType.MissingPasswordException;
        }
        else if (message.StartsWith("MISSING_REFRESH_TOKEN"))
        {
            errorType = AuthErrorType.MissingRefreshTokenException;
        }
        else if (message.StartsWith("MISSING_REQ_TYPE"))
        {
            errorType = AuthErrorType.MissingRequestTypeException;
        }
        else if (message.StartsWith("MISSING_REQUEST_URI"))
        {
            errorType = AuthErrorType.MissingRequestURIException;
        }
        else if (message.StartsWith("OPERATION_NOT_ALLOWED"))
        {
            errorType = AuthErrorType.OperationNotAllowedException;
        }
        else if (message.StartsWith("RESET_PASSWORD_EXCEED_LIMIT"))
        {
            errorType = AuthErrorType.ResetPasswordExceedLimitException;
        }
        else if (message.StartsWith("ERROR_INVALID_CREDENTIAL"))
        {
            errorType = AuthErrorType.StaleIDTokenException;
        }
        else if (message.StartsWith("A system error has occurred"))
        {
            errorType = AuthErrorType.SystemErrorException;
        }
        else if (message.StartsWith("TOKEN_EXPIRED"))
        {
            errorType = AuthErrorType.TokenExpiredException;
        }
        else if (message.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER"))
        {
            errorType = AuthErrorType.TooManyAttemptsException;
        }
        else if (message.StartsWith("USER_DISABLED"))
        {
            errorType = AuthErrorType.UserDisabledException;
        }
        else if (message.StartsWith("USER_NOT_FOUND"))
        {
            errorType = AuthErrorType.UserNotFoundException;
        }
        else if (message.StartsWith("WEAK_PASSWORD"))
        {
            errorType = AuthErrorType.WeakPasswordException;
        }
        else
        {
            errorType = AuthErrorType.UndefinedException;
        }

        return new FirebaseAuthenticationException(errorType, message ?? "Unknown error occured.", requestUrlStr, requestContentStr, responseStr, httpStatusCode, exception);
    }

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

    internal async Task<(string? response, Exception? exception)> ExecuteWithPostContent(string postContent, string googleUrl)
    {
        var (response, exception) = await ExecuteWithContent(postContent, HttpMethod.Post, BuildUrl(googleUrl));
        if (response == null)
        {
            return (null, exception);
        }

        var responseData = await response.Content.ReadAsStringAsync();
        return (responseData, null);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<(T? response, Exception? exception)> ExecuteWithGet<T>(string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        var (response, exception) = await Execute(HttpMethod.Get, BuildUrl(googleUrl));
        if (response == null)
        {
            return (default, exception);
        }

        var responseData = await response.Content.ReadAsStringAsync();
        return (JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions), null);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<(T? response, Exception? exception)> ExecuteWithPostContent<T>(string postContent, string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        var (response, exception) = await ExecuteWithContent(postContent, HttpMethod.Post, BuildUrl(googleUrl));
        if (response == null)
        {
            return (default, exception);
        }

        var responseData = await response.Content.ReadAsStringAsync();
        return (JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions), null);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<(FirebaseAuth? auth, Exception? exception)> ExecuteAuthWithPostContent(string postContent, string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        var (response, exception) = await ExecuteWithPostContent<FirebaseAuth>(postContent, BuildUrl(googleUrl), jsonSerializerOptions);
        if (response == null)
        {
            return (default, exception);
        }

        return (response, null);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<Exception?> RefreshUserInfo(FirebaseUser user)
    {
        var content = $"{{\"idToken\":\"{user.IdToken}\"}}";

        var (executeResult, executeException) = await ExecuteWithContent(content, HttpMethod.Post, BuildUrl(GoogleGetUser));
        if (executeResult == null)
        {
            return executeException;
        }

        var responseData = await executeResult.Content.ReadAsStringAsync();

        JsonDocument resultJson = JsonDocument.Parse(responseData);
        if (!resultJson.RootElement.TryGetProperty("users", out JsonElement userJson))
        {
            throw new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default);
        }

        var auth = JsonSerializer.Deserialize<FirebaseAuth>(userJson.EnumerateArray().First(), CamelCaseJsonSerializerOption);

        if (auth == null)
        {
            throw new FirebaseAuthenticationException(AuthErrorType.UndefinedException, "Unknown error occured.", default, default, default, default, default);
        }

        user.UpdateAuth(auth);
        user.UpdateInfo(auth);

        return null;
    }
}
