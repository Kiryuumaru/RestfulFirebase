using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when an invalid refresh token is provided.
/// </summary>
public class AuthInvalidRefreshTokenException : AuthException
{
    private const string ExceptionMessage =
        "An invalid refresh token is provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidRefreshTokenException"/>.
    /// </summary>
    public AuthInvalidRefreshTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidRefreshTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidRefreshTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
