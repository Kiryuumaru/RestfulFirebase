using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the supplied auth credential is malformed or has expired.
/// </summary>
public class AuthInvalidIDPResponseException : AuthException
{
    private const string ExceptionMessage =
        "The supplied auth credential is malformed or has expired.";

    internal AuthInvalidIDPResponseException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidIDPResponseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
