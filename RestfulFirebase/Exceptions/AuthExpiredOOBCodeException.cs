using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the action code has expired.
/// </summary>
public class AuthExpiredOOBCodeException : AuthException
{
    private const string ExceptionMessage =
        "The action code has expired.";

    internal AuthExpiredOOBCodeException()
        : base(ExceptionMessage)
    {

    }

    internal AuthExpiredOOBCodeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
