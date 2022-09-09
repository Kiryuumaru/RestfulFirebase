using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the custom token corresponds to a different Firebase project.
/// </summary>
public class AuthCredentialMismatchException : AuthException
{
    private const string ExceptionMessage =
        "The custom token corresponds to a different Firebase project.";

    /// <summary>
    /// Creates an instance of <see cref="AuthCredentialMismatchException"/>.
    /// </summary>
    public AuthCredentialMismatchException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthCredentialMismatchException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthCredentialMismatchException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
