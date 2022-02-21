using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when an email address was expected but one was not provided.
/// </summary>
public class AuthMissingEmailException : AuthException
{
    private const string ExceptionMessage =
        "Email address was expected but one was not provided.";

    internal AuthMissingEmailException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingEmailException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
