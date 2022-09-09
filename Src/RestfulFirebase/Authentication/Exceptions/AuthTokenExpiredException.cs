using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the user's credential is no longer valid. The user must sign in again.
/// </summary>
public class AuthTokenExpiredException : AuthException
{
    private const string ExceptionMessage =
        "The user's credential is no longer valid. The user must sign in again.";

    /// <summary>
    /// Creates an instance of <see cref="AuthTokenExpiredException"/>.
    /// </summary>
    public AuthTokenExpiredException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthTokenExpiredException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthTokenExpiredException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
