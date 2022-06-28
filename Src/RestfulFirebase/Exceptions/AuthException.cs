using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in authentication.
/// </summary>
public class AuthException : Exception
{
    private const string ExceptionMessage =
        "An authentication error occured.";

    /// <summary>
    /// Creates an instance of <see cref="AuthException"/>.
    /// </summary>
    public AuthException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthException"/> with provided <paramref name="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    public AuthException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthException"/> with provided <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
