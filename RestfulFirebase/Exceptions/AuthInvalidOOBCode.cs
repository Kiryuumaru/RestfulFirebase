using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the action code is invalid. This can happen if the code is malformed, expired, or has already been used.
/// </summary>
public class AuthInvalidOOBCodeException : AuthException
{
    private const string ExceptionMessage =
        "The action code is invalid. This can happen if the code is malformed, expired, or has already been used.";

    internal AuthInvalidOOBCodeException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidOOBCodeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
