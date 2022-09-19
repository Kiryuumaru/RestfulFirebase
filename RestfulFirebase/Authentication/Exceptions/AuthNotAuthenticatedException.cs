using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when app is not authenticated.
/// </summary>
public class AuthNotAuthenticatedException : AuthException
{
    private const string ExceptionMessage =
        "App is not authenticated.";

    /// <summary>
    /// Creates an instance of <see cref="AuthNotAuthenticatedException"/>.
    /// </summary>
    public AuthNotAuthenticatedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthNotAuthenticatedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthNotAuthenticatedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
