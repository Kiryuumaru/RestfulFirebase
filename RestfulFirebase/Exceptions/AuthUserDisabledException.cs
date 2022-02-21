using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the user account has been disabled by an administrator.
/// </summary>
public class AuthUserDisabledException : AuthException
{
    private const string ExceptionMessage =
        "The user account has been disabled by an administrator.";

    internal AuthUserDisabledException()
        : base(ExceptionMessage)
    {

    }

    internal AuthUserDisabledException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
