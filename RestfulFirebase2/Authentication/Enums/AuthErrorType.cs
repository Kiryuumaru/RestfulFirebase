namespace RestfulFirebase.Authentication.Enums;

/// <summary>
/// The type of authentication exception. 
/// </summary>
public enum AuthErrorType
{
    /// <summary>
    /// Occurs when the specified credential is already associated with a different user account.
    /// </summary>
    AlreadyLinkedException,

    /// <summary>
    /// Occurs when the provided API key is not valid.
    /// </summary>
    APIKeyNotValidException,

    /// <summary>
    /// Occurs when the custom token corresponds to a different Firebase project.
    /// </summary>
    CredentialMismatchException,

    /// <summary>
    /// Occurs when the API request was received with a repeated or lower than expected nonce value.
    /// </summary>
    DuplicateCredentialUseException,

    /// <summary>
    /// Occurs when the email address is already in use by another account.
    /// </summary>
    EmailExistsException,

    /// <summary>
    /// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
    /// </summary>
    EmailNotFoundException,

    /// <summary>
    /// Occurs when the action code has expired.
    /// </summary>
    ExpiredOOBCodeException,

    /// <summary>
    /// Occurs when either the user or API keys are incorrect, or the API key has expired.
    /// </summary>
    InvalidAccessTokenException,

    /// <summary>
    /// Occurs when the custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
    /// </summary>
    InvalidCustomTokenException,

    /// <summary>
    /// Occurs when the email address is badly formatted.
    /// </summary>
    InvalidEmailAddressException,

    /// <summary>
    /// Occurs when the grant type specified is invalid.
    /// </summary>
    InvalidGrantTypeException,

    /// <summary>
    /// Occurs when the provided identifier is invalid.
    /// </summary>
    InvalidIdentifierException,

    /// <summary>
    /// Occurs when the supplied auth credential is malformed or has expired.
    /// </summary>
    InvalidIDPResponseException,

    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    InvalidIDTokenException,

    /// <summary>
    /// Occurs when there`s an invalid JSON payload received.
    /// </summary>
    InvalidJSONReceivedException,

    /// <summary>
    /// Occurs when the action code is invalid. This can happen if the code is malformed, expired, or has already been used.
    /// </summary>
    InvalidOOBCodeException,

    /// <summary>
    /// Occurs when the password is invalid or the user does not have a password.
    /// </summary>
    InvalidPasswordException,

    /// <summary>
    /// Occurs when the supported provider identifier string is not a valid providerId.
    /// </summary>
    InvalidProviderIDException,

    /// <summary>
    /// Occurs when an invalid refresh token is provided.
    /// </summary>
    InvalidRefreshTokenException,

    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    LoginCredentialsTooOldException,

    /// <summary>
    /// Occurs when an email address was expected but one was not provided.
    /// </summary>
    MissingEmailException,

    /// <summary>
    /// Occurs when an identifier was expected but one was not provided.
    /// </summary>
    MissingIdentifierException,

    /// <summary>
    /// Occurs when password was expected but one was not provided.
    /// </summary>
    MissingPasswordException,

    /// <summary>
    /// Occurs when token was expected but one was not provided.
    /// </summary>
    MissingRefreshTokenException,

    /// <summary>
    /// Occurs when request type was expected but one was not provided.
    /// </summary>
    MissingRequestTypeException,

    /// <summary>
    /// Occurs when request uri was expected but one was not provided.
    /// </summary>
    MissingRequestURIException,

    /// <summary>
    /// Occurs when app is not authenticated.
    /// </summary>
    NotAuthenticatedException,

    /// <summary>
    /// Occurs when an specified operation is disabled for this project.
    /// </summary>
    OperationNotAllowedException,

    /// <summary>
    /// Occurs when the reset password request exceeds its limit.
    /// </summary>
    ResetPasswordExceedLimitException,

    /// <summary>
    /// Occurs when the supplied auth credential is malformed or has expired.
    /// </summary>
    StaleIDTokenException,

    /// <summary>
    /// Occurs when the system has error.
    /// </summary>
    SystemErrorException,

    /// <summary>
    /// Occurs when the user's credential is no longer valid. The user must sign in again.
    /// </summary>
    TokenExpiredException,

    /// <summary>
    /// Occurs when there is an unusual activity on device.
    /// </summary>
    TooManyAttemptsException,

    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    UndefinedException,

    /// <summary>
    /// Occurs when the user account has been disabled by an administrator.
    /// </summary>
    UserDisabledException,

    /// <summary>
    /// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
    /// </summary>
    UserNotFoundException,

    /// <summary>
    /// Occurs when the password is less than 6 characters long.
    /// </summary>
    WeakPasswordException,
} 
