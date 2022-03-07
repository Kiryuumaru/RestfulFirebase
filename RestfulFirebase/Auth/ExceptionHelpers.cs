using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Text.Json;

namespace RestfulFirebase.Auth;

internal class ExceptionHelpers
{
    internal static Exception GetException(string responseData, Exception originalException)
    {
        string message = "";
        try
        {
            if (!string.IsNullOrEmpty(responseData) && responseData != "N/A")
            {
                //create error data template and try to parse JSON
                var errorData = new { error = new { code = 0, message = "errorid" } };
                errorData = JsonSerializerExtensions.DeserializeAnonymousType(responseData, errorData);

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
}
