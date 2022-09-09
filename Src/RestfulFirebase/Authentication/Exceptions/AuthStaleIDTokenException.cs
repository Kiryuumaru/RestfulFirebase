using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the supplied auth credential is malformed or has expired.
/// </summary>
public class AuthStaleIDTokenException : AuthException
{
    private const string ExceptionMessage =
        "The supplied auth credential is malformed or has expired.";

    /// <summary>
    /// Creates an instance of <see cref="AuthStaleIDTokenException"/>.
    /// </summary>
    public AuthStaleIDTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthStaleIDTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthStaleIDTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
