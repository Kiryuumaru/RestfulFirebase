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
using RestfulFirebase.Common.Transactions;
using System.Net;
using System.Runtime.CompilerServices;
using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Authentication.Transactions;

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
        string message = "";
        try
        {
            string? responseData = null;
            if (response != null)
            {
               responseData = await response.Content.ReadAsStringAsync();
            }
            if (responseData != null && !string.IsNullOrEmpty(responseData) && responseData != "N/A")
            {
                //create error data template and try to parse JSON
                ErrorData? errorData = JsonSerializer.Deserialize<ErrorData>(responseData, CamelCaseJsonSerializerOption);

                //errorData is just null if different JSON was received
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

        if (message.StartsWith("invalid access_token, error code 43."))
        {
            return new AuthInvalidAccessTokenException(exception);
        }
        else if (message.StartsWith("API key not valid"))
        {
            return new AuthAPIKeyNotValidException(exception);
        }
        else if (message.StartsWith("A system error has occurred"))
        {
            return new AuthSystemErrorException(exception);
        }
        else if (message.StartsWith("Invalid JSON payload received"))
        {
            return new AuthInvalidJSONReceivedException(exception);
        }
        else if (message.StartsWith("CREDENTIAL_TOO_OLD_LOGIN_AGAIN"))
        {
            return new AuthLoginCredentialsTooOldException(exception);
        }
        else if (message.StartsWith("OPERATION_NOT_ALLOWED"))
        {
            return new AuthOperationNotAllowedException(exception);
        }
        else if (message.StartsWith("INVALID_PROVIDER_ID"))
        {
            return new AuthInvalidProviderIDException(exception);
        }
        else if (message.StartsWith("MISSING_REQUEST_URI"))
        {
            return new AuthMissingRequestURIException(exception);
        }
        else if (message.StartsWith("MISSING_OR_INVALID_NONCE"))
        {
            return new AuthDuplicateCredentialUseException(exception);
        }
        else if (message.StartsWith("INVALID_CUSTOM_TOKEN"))
        {
            return new AuthInvalidCustomTokenException(exception);
        }
        else if (message.StartsWith("CREDENTIAL_MISMATCH"))
        {
            return new AuthCredentialMismatchException(exception);
        }
        else if (message.StartsWith("INVALID_EMAIL"))
        {
            return new AuthInvalidEmailAddressException(exception);
        }
        else if (message.StartsWith("MISSING_PASSWORD"))
        {
            return new AuthMissingPasswordException(exception);
        }
        else if (message.StartsWith("EMAIL_EXISTS"))
        {
            return new AuthEmailExistsException(exception);
        }
        else if (message.StartsWith("USER_NOT_FOUND"))
        {
            return new AuthUserNotFoundException(exception);
        }
        else if (message.StartsWith("INVALID_PASSWORD"))
        {
            return new AuthInvalidPasswordException(exception);
        }
        else if (message.StartsWith("EMAIL_NOT_FOUND"))
        {
            return new AuthEmailNotFoundException(exception);
        }
        else if (message.StartsWith("USER_DISABLED"))
        {
            return new AuthUserDisabledException(exception);
        }
        else if (message.StartsWith("MISSING_EMAIL"))
        {
            return new AuthMissingEmailException(exception);
        }
        else if (message.StartsWith("RESET_PASSWORD_EXCEED_LIMIT"))
        {
            return new AuthResetPasswordExceedLimitException(exception);
        }
        else if (message.StartsWith("MISSING_REQ_TYPE"))
        {
            return new AuthMissingRequestTypeException(exception);
        }
        else if (message.StartsWith("INVALID_ID_TOKEN"))
        {
            return new AuthInvalidIDTokenException(exception);
        }
        else if (message.StartsWith("INVALID_IDENTIFIER"))
        {
            return new AuthInvalidIdentifierException(exception);
        }
        else if (message.StartsWith("MISSING_IDENTIFIER"))
        {
            return new AuthMissingIdentifierException(exception);
        }
        else if (message.StartsWith("FEDERATED_USER_ID_ALREADY_LINKED"))
        {
            return new AuthAlreadyLinkedException(exception);
        }
        else if (message.StartsWith("TOKEN_EXPIRED"))
        {
            return new AuthTokenExpiredException(exception);
        }
        else if (message.StartsWith("INVALID_REFRESH_TOKEN"))
        {
            return new AuthInvalidRefreshTokenException(exception);
        }
        else if (message.StartsWith("INVALID_GRANT_TYPE"))
        {
            return new AuthInvalidGrantTypeException(exception);
        }
        else if (message.StartsWith("MISSING_REFRESH_TOKEN"))
        {
            return new AuthMissingRefreshTokenException(exception);
        }
        else if (message.StartsWith("WEAK_PASSWORD"))
        {
            return new AuthWeakPasswordException(exception);
        }
        else if (message.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER"))
        {
            return new AuthTooManyAttemptsException(exception);
        }
        else if (message.StartsWith("ERROR_INVALID_CREDENTIAL"))
        {
            return new AuthStaleIDTokenException(exception);
        }
        else if (message.StartsWith("INVALID_IDP_RESPONSE"))
        {
            return new AuthInvalidIDPResponseException(exception);
        }
        else if (message.StartsWith("EXPIRED_OOB_CODE"))
        {
            return new AuthExpiredOOBCodeException(exception);
        }
        else if (message.StartsWith("INVALID_OOB_CODE"))
        {
            return new AuthInvalidOOBCodeException(exception);
        }
        else
        {
            return new AuthUndefinedException(exception);
        }
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

    internal async Task<string?> ExecuteWithPostContent(string postContent, string googleUrl)
    {
        var response = await ExecuteWithContent(postContent, HttpMethod.Post, BuildUrl(googleUrl));
        var responseData = await response.Content.ReadAsStringAsync();

        return responseData;
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<T?> ExecuteWithGet<T>(string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        var response = await Execute(HttpMethod.Get, BuildUrl(googleUrl));
        var responseData = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<T?> ExecuteWithPostContent<T>(string postContent, string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        var response = await ExecuteWithContent(postContent, HttpMethod.Post, BuildUrl(googleUrl));
        var responseData = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(responseData, jsonSerializerOptions);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<FirebaseAuth> ExecuteAuthWithPostContent(string postContent, string googleUrl, JsonSerializerOptions jsonSerializerOptions)
    {
        FirebaseAuth? auth = await ExecuteWithPostContent<FirebaseAuth>(postContent, BuildUrl(googleUrl), jsonSerializerOptions);

        if (auth == null)
        {
            throw new AuthUndefinedException();
        }

        return auth;
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task RefreshUserInfo(FirebaseUser user)
    {
        var content = $"{{\"idToken\":\"{user.IdToken}\"}}";

        var response = await ExecuteWithContent(content, HttpMethod.Post, BuildUrl(GoogleGetUser));
        var responseData = await response.Content.ReadAsStringAsync();

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
}
