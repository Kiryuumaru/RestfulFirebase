using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the provided API key is not valid.
/// </summary>
public class AuthAPIKeyNotValidException : AuthException
{
    private const string ExceptionMessage =
        "API key is not valid. Please pass a valid API key.";

    /// <summary>
    /// Creates an instance of <see cref="AuthAPIKeyNotValidException"/>.
    /// </summary>
    public AuthAPIKeyNotValidException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthAPIKeyNotValidException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthAPIKeyNotValidException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
