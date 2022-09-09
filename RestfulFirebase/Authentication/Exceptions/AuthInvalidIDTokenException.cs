using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthInvalidIDTokenException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIDTokenException"/>.
    /// </summary>
    public AuthInvalidIDTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIDTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidIDTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
