using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided identifier is invalid.
/// </summary>
public class AuthInvalidIdentifierException : AuthException
{
    private const string ExceptionMessage =
        "The provided identifier is invalid.";

    internal AuthInvalidIdentifierException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidIdentifierException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
