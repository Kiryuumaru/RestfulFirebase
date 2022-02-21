using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthInvalidIDTokenException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    internal AuthInvalidIDTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidIDTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
