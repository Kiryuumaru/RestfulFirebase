using System;
using System.Net;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    public enum FirebaseExceptionReason
    {
        #region AuthError

        /// <summary>
        /// Unknown error reason.
        /// </summary>
        AuthUndefined,
        /// <summary>
        /// The sign in method is not enabled.
        /// </summary>
        AuthOperationNotAllowed,
        /// <summary>
        /// The user was disabled and is not granted access anymore.
        /// </summary>
        AuthUserDisabled,
        /// <summary>
        /// The user was not found
        /// </summary>
        AuthUserNotFound,
        /// <summary>
        /// Third-party Auth Providers: PostBody does not contain or contains invalid Authentication Provider string.
        /// </summary>
        AuthInvalidProviderID,
        /// <summary>
        /// Third-party Auth Providers: PostBody does not contain or contains invalid Access Token string obtained from Auth Provider.
        /// </summary>
        AuthInvalidAccessToken,
        /// <summary>
        /// Changes to user's account has been made since last log in. User needs to login again.
        /// </summary>
        AuthLoginCredentialsTooOld,
        /// <summary>
        /// Third-party Auth Providers: Request does not contain a value for parameter: requestUri.
        /// </summary>
        AuthMissingRequestURI,
        /// <summary>
        /// Third-party Auth Providers: Request does not contain a value for parameter: postBody.
        /// </summary>
        AuthSystemError,
        /// <summary>
        /// Email/Password Authentication: Email address is not in valid format.
        /// </summary>
        AuthInvalidEmailAddress,
        /// <summary>
        /// Email/Password Authentication: No password provided!
        /// </summary>
        AuthMissingPassword,
        /// <summary>
        /// Email/Password Signup: Password must be more than 6 characters.  This error could also be caused by attempting to create a user account using Set Account Info without supplying a password for the new user.
        /// </summary>
        AuthWeakPassword,
        /// <summary>
        /// Email/Password Signup: Email address already connected to another account. This error could also be caused by attempting to create a user account using Set Account Info and an email address already linked to another account.
        /// </summary>
        AuthEmailExists,
        /// <summary>
        /// Email/Password Signin: No email provided! This error could also be caused by attempting to create a user account using Set Account Info without supplying an email for the new user.
        /// </summary>
        AuthMissingEmail,
        /// <summary>
        /// Email/Password Signin: No user with a matching email address is registered.
        /// </summary>
        AuthUnknownEmailAddress,
        /// <summary>
        /// Email/Password Signin: The supplied password is not valid for the email address.
        /// </summary>
        AuthWrongPassword,
        /// <summary>
        /// Email/Password Signin: Too many password login have been attempted. Try again later.
        /// </summary>
        AuthTooManyAttemptsTryLater,
        /// <summary>
        /// Password Recovery: Request does not contain a value for parameter: requestType or supplied value is invalid.
        /// </summary>
        AuthMissingRequestType,
        /// <summary>
        /// Password Recovery: Reset password limit exceeded.
        /// </summary>
        AuthResetPasswordExceedLimit,
        /// <summary>
        /// Account Linking: Authenticated User ID Token is invalid!
        /// </summary>
        AuthInvalidIDToken,
        /// <summary>
        /// Linked Accounts: Request does not contain a value for parameter: identifier.
        /// </summary>
        AuthMissingIdentifier,
        /// <summary>
        /// Linked Accounts: Request contains an invalid value for parameter: identifier.
        /// </summary>
        AuthInvalidIdentifier,
        /// <summary>
        /// Linked accounts: account to link has already been linked.
        /// </summary>
        AuthAlreadyLinked,

        //Customs

        /// <summary>
        /// User not authenticated
        /// </summary>
        AuthNotAuthenticated,

        /// <summary>
        /// Auth error.
        /// </summary>
        Auth =
            AuthUndefined |
            AuthOperationNotAllowed |
            AuthUserDisabled |
            AuthUserNotFound |
            AuthInvalidProviderID |
            AuthInvalidAccessToken |
            AuthLoginCredentialsTooOld |
            AuthMissingRequestURI |
            AuthSystemError |
            AuthInvalidEmailAddress |
            AuthMissingPassword |
            AuthWeakPassword |
            AuthEmailExists |
            AuthMissingEmail |
            AuthUnknownEmailAddress |
            AuthWrongPassword |
            AuthTooManyAttemptsTryLater |
            AuthMissingRequestType |
            AuthResetPasswordExceedLimit |
            AuthInvalidIDToken |
            AuthMissingIdentifier |
            AuthInvalidIdentifier |
            AuthAlreadyLinked |
            AuthNotAuthenticated,

        #endregion

        #region DatabaseError

        /// <summary>
        /// Unknown error reason.
        /// </summary>
        DatabaseUndefined,
        /// <summary>
        /// Bad request.
        /// </summary>
        DatabaseBadRequest,
        /// <summary>
        /// Request not authorized by database rules.
        /// </summary>
        DatabaseUnauthorized,
        /// <summary>
        /// The specified Realtime Database was not found.
        /// </summary>
        DatabaseNotFound,
        /// <summary>
        /// The server returned an error.
        /// </summary>
        DatabaseInternalServerError,
        /// <summary>
        /// The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
        /// </summary>
        DatabaseServiceUnavailable,
        /// <summary>
        /// The request's specified ETag value in the if-match header did not match the server's value.
        /// </summary>
        DatabasePreconditionFailed,

        /// <summary>
        /// Database error.
        /// </summary>
        Database =
            DatabaseUndefined |
            DatabaseBadRequest |
            DatabaseUnauthorized |
            DatabaseNotFound |
            DatabaseInternalServerError |
            DatabaseServiceUnavailable |
            DatabasePreconditionFailed,

        #endregion

        #region StorageError

        /// <summary>
        /// Unknown error reason.
        /// </summary>
        StorageUndefined,
        /// <summary>
        /// Request not authorized by storage rules.
        /// </summary>
        StorageUnauthorized,

        /// <summary>
        /// Storage error.
        /// </summary>
        Storage =
            StorageUndefined |
            StorageUnauthorized,

        #endregion

        /// <summary>
        /// Undefined error.
        /// </summary>
        Undefined =
            AuthUndefined |
            DatabaseUndefined |
            StorageUndefined,

        /// <summary>
        /// Operation is cancelled.
        /// </summary>
        OperationCancelled,
        /// <summary>
        /// Config offline mode.
        /// </summary>
        OfflineMode
    }
}
