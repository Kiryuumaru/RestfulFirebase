using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the password is invalid or the user does not have a password.
/// </summary>
public class AuthInvalidPasswordException : AuthException
{
    private const string ExceptionMessage =
        "The password is invalid or the user does not have a password.";

    internal AuthInvalidPasswordException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
