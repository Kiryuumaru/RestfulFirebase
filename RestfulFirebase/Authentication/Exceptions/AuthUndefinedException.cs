using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class AuthUndefinedException : AuthException
{
    private const string ExceptionMessage =
        "An unidentified exception occurs.";

    /// <summary>
    /// Creates an instance of <see cref="AuthUndefinedException"/>.
    /// </summary>
    public AuthUndefinedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthUndefinedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthUndefinedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
