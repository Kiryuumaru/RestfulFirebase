using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the supplied auth credential is malformed or has expired.
/// </summary>
public class AuthInvalidIDPResponseException : AuthException
{
    private const string ExceptionMessage =
        "The supplied auth credential is malformed or has expired.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIDPResponseException"/>.
    /// </summary>
    public AuthInvalidIDPResponseException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIDPResponseException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidIDPResponseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
