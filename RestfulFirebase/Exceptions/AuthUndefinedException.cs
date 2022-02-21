using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class AuthUndefinedException : AuthException
{
    private const string ExceptionMessage =
        "An unidentified exception occurs.";

    internal AuthUndefinedException()
        : base(ExceptionMessage)
    {

    }

    internal AuthUndefinedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
