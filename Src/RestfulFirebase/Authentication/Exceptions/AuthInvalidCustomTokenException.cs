using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)
/// </summary>
public class AuthInvalidCustomTokenException : AuthException
{
    private const string ExceptionMessage =
        "The custom token format is incorrect or the token is invalid for some reason (e.g. expired, invalid signature etc.)";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidCustomTokenException"/>.
    /// </summary>
    public AuthInvalidCustomTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidCustomTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidCustomTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
