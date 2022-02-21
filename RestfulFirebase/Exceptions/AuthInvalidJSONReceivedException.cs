using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an invalid JSON payload received.
/// </summary>
public class AuthInvalidJSONReceivedException : AuthException
{
    private const string ExceptionMessage =
        "Invalid JSON payload received.";

    internal AuthInvalidJSONReceivedException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidJSONReceivedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
