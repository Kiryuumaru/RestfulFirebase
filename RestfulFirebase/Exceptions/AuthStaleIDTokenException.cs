using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the supplied auth credential is malformed or has expired.
/// </summary>
public class AuthStaleIDTokenException : AuthException
{
    private const string ExceptionMessage =
        "The supplied auth credential is malformed or has expired.";

    internal AuthStaleIDTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthStaleIDTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
