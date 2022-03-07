using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an invalid JSON payload received.
/// </summary>
public class AuthInvalidJSONReceivedException : AuthException
{
    private const string ExceptionMessage =
        "Invalid JSON payload received.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidJSONReceivedException"/>.
    /// </summary>
    public AuthInvalidJSONReceivedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidJSONReceivedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidJSONReceivedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
