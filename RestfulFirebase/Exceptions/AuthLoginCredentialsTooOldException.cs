using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthLoginCredentialsTooOldException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    internal AuthLoginCredentialsTooOldException()
        : base(ExceptionMessage)
    {

    }

    internal AuthLoginCredentialsTooOldException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
