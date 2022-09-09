using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there is an unusual activity on device.
/// </summary>
public class AuthTooManyAttemptsException : AuthException
{
    private const string ExceptionMessage =
        "We have blocked all requests from this device due to unusual activity. Try again later.";

    /// <summary>
    /// Creates an instance of <see cref="AuthTooManyAttemptsException"/>.
    /// </summary>
    public AuthTooManyAttemptsException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthTooManyAttemptsException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthTooManyAttemptsException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
