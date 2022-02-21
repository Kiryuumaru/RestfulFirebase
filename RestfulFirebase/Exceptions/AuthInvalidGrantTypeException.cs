using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the grant type specified is invalid.
/// </summary>
public class AuthInvalidGrantTypeException : AuthException
{
    private const string ExceptionMessage =
        "The grant type specified is invalid.";

    internal AuthInvalidGrantTypeException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidGrantTypeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
