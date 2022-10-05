using System.Threading.Tasks;
using RestfulFirebase.Authentication.Internals;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common.Internals;
using System.Net;
using System;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Enums;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.IO;
using System.Linq;

namespace RestfulFirebase.Authentication;

public partial class AuthenticationApi
{
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

    internal string BuildUrl(string googleUrl)
    {
        return string.Format(googleUrl, App.Config.ApiKey);
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ErrorData))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal static async Task<Exception> GetHttpException(IHttpResponse response)
    {
        var lastTransaction = response.HttpTransactions.LastOrDefault();

        string? requestUrlStr = lastTransaction?.HttpRequestMessage?.RequestUri?.ToString();
        string? requestContentStr = lastTransaction == null ? null : await lastTransaction.GetRequestContentAsString();
        string? responseContentStr = lastTransaction == null ? null : await lastTransaction.GetResponseContentAsString();

        string? message = null;
        try
        {
            if (responseContentStr != null && !string.IsNullOrEmpty(responseContentStr) && responseContentStr != "N/A")
            {
                ErrorData? errorData = JsonSerializer.Deserialize<ErrorData>(responseContentStr, JsonSerializerHelpers.CamelCaseJsonSerializerOption);
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

        return new FirebaseAuthenticationException(errorType, message ?? "Unknown error occured.", requestUrlStr, requestContentStr, responseContentStr, lastTransaction?.HttpStatusCode, response.Error);
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode("Calls RestfulFirebase.Common.Http.HttpHelpers.ExecuteWithContent<T>(HttpClient, Stream, HttpMethod, String, JsonSerializerOptions, CancellationToken)")]
#endif
    internal async Task<HttpResponse<T>> ExecuteGet<T>(string googleUrl, CancellationToken cancellationToken)
    {
        var response = await HttpHelpers.Execute<T>(App.GetClient(), HttpMethod.Get, BuildUrl(googleUrl), JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        if (response.IsError)
        {
            return new(default, response, await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePost(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        var response = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, BuildUrl(googleUrl), cancellationToken);
        if (response.IsError)
        {
            return new(response, await GetHttpException(response));
        }

        return response;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode("Calls RestfulFirebase.Common.Http.HttpHelpers.ExecuteWithContent<T>(HttpClient, Stream, HttpMethod, String, JsonSerializerOptions, CancellationToken)")]
#endif
    internal async Task<HttpResponse<T>> ExecutePost<T>(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        var response = await HttpHelpers.ExecuteWithContent<T>(App.GetClient(), stream, HttpMethod.Post, BuildUrl(googleUrl), JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        if (response.IsError)
        {
            return new(default, response, await GetHttpException(response));
        }

        return response;
    }

#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FirebaseAuth))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
    internal async Task<HttpResponse<FirebaseUser>> StartUser(MemoryStream stream, string googleUrl, CancellationToken cancellationToken)
    {
        HttpResponse<FirebaseUser> response = new();

        var postResponse = await ExecutePost<FirebaseAuth>(stream, googleUrl, cancellationToken);
        response.Concat(postResponse);
        if (postResponse.IsError)
        {
            return response;
        }

        FirebaseUser user = new(App, postResponse.Result);
        response.Concat(user);

        var refreshResponse = await user.RefreshUserInfo(cancellationToken);
        response.Concat(refreshResponse);
        if (refreshResponse.IsError)
        {
            return response;
        }

        return response;
    }
}
