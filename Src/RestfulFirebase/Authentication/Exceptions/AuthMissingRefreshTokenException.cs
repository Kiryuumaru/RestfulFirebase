using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when token was expected but one was not provided.
/// </summary>
public class AuthMissingRefreshTokenException : AuthException
{
    private const string ExceptionMessage =
        "Token was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRefreshTokenException"/>.
    /// </summary>
    public AuthMissingRefreshTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingRefreshTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingRefreshTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
