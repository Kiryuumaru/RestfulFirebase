using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when an identifier was expected but one was not provided.
/// </summary>
public class AuthMissingIdentifierException : AuthException
{
    private const string ExceptionMessage =
        "Identifier was expected but one was not provided.";

    internal AuthMissingIdentifierException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingIdentifierException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
