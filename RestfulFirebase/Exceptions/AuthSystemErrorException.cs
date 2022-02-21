using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the system has error.
/// </summary>
public class AuthSystemErrorException : AuthException
{
    private const string ExceptionMessage =
        "A system error has occurred.";

    internal AuthSystemErrorException()
        : base(ExceptionMessage)
    {

    }

    internal AuthSystemErrorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
