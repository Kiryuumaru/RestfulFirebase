using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when password was expected but one was not provided.
/// </summary>
public class AuthMissingPasswordException : AuthException
{
    private const string ExceptionMessage =
        "Password was expected but one was not provided.";

    internal AuthMissingPasswordException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
