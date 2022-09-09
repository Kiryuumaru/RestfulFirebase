using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the provided identifier is invalid.
/// </summary>
public class AuthInvalidIdentifierException : AuthException
{
    private const string ExceptionMessage =
        "The provided identifier is invalid.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIdentifierException"/>.
    /// </summary>
    public AuthInvalidIdentifierException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidIdentifierException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidIdentifierException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
