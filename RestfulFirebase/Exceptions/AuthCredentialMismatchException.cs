using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the custom token corresponds to a different Firebase project.
/// </summary>
public class AuthCredentialMismatchException : AuthException
{
    private const string ExceptionMessage =
        "The custom token corresponds to a different Firebase project.";

    internal AuthCredentialMismatchException()
        : base(ExceptionMessage)
    {

    }

    internal AuthCredentialMismatchException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
