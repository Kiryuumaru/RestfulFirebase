using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
/// </summary>
public class AuthInvalidCustomTokenException : AuthException
{
    private const string ExceptionMessage =
        "The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)";

    internal AuthInvalidCustomTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidCustomTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
