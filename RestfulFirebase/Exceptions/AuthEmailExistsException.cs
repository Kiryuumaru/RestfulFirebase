using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the email address is already in use by another account.
/// </summary>
public class AuthEmailExistsException : AuthException
{
    private const string ExceptionMessage =
        "The email address is already in use by another account.";

    internal AuthEmailExistsException()
        : base(ExceptionMessage)
    {

    }

    internal AuthEmailExistsException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
