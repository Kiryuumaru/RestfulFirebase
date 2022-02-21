using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when request uri was expected but one was not provided.
/// </summary>
public class AuthMissingRequestURIException : AuthException
{
    private const string ExceptionMessage =
        "Request uri was expected but one was not provided.";

    internal AuthMissingRequestURIException()
        : base(ExceptionMessage)
    {

    }

    internal AuthMissingRequestURIException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
