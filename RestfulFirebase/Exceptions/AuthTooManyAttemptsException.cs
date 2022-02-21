using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there is an unusual activity on device.
/// </summary>
public class AuthTooManyAttemptsException : AuthException
{
    private const string ExceptionMessage =
        "We have blocked all requests from this device due to unusual activity. Try again later.";

    internal AuthTooManyAttemptsException()
        : base(ExceptionMessage)
    {

    }

    internal AuthTooManyAttemptsException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
