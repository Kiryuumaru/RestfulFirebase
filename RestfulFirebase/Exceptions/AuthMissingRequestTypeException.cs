using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when request type was expected but one was not provided.
/// </summary>
public class AuthMissingRequestTypeException : AuthException
{
    private const string ExceptionMessage =
        "Request type was expected but one was not provided.";

    internal AuthMissingRequestTypeException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingRequestTypeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
