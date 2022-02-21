using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthTokenExpiredException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    internal AuthTokenExpiredException()
        : base(ExceptionMessage)
    {

    }

    internal AuthTokenExpiredException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
