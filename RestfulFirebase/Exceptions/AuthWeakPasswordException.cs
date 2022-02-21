using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the password is less than 6 characters long.
/// </summary>
public class AuthWeakPasswordException : AuthException
{
    private const string ExceptionMessage =
        "The password must be 6 characters long or more.";

    internal AuthWeakPasswordException()
        : base(ExceptionMessage)
    {

    }

    internal AuthWeakPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
