using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when token was expected but one was not provided.
/// </summary>
public class AuthMissingRefreshTokenException : AuthException
{
    private const string ExceptionMessage =
        "Token was expected but one was not provided.";

    internal AuthMissingRefreshTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingRefreshTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
