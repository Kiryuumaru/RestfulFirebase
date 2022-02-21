using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when an invalid refresh token is provided.
/// </summary>
public class AuthInvalidRefreshTokenException : AuthException
{
    private const string ExceptionMessage =
        "An invalid refresh token is provided.";

    internal AuthInvalidRefreshTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidRefreshTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
