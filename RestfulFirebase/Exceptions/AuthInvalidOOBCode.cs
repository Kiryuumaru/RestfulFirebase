using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the action code is invalid. This can happen if the code is malformed, expired, or has already been used.
/// </summary>
public class AuthInvalidOOBCodeException : AuthException
{
    private const string ExceptionMessage =
        "The action code is invalid. This can happen if the code is malformed, expired, or has already been used.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidOOBCodeException"/>.
    /// </summary>
    public AuthInvalidOOBCodeException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidOOBCodeException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidOOBCodeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
