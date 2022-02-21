using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when app is not authenticated.
/// </summary>
public class AuthNotAuthenticatedException : AuthException
{
    private const string ExceptionMessage =
        "App is not authenticated.";

    internal AuthNotAuthenticatedException()
        : base(ExceptionMessage)
    {

    }

    internal AuthNotAuthenticatedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
