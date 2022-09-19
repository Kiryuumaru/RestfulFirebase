using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the grant type specified is invalid.
/// </summary>
public class AuthInvalidGrantTypeException : AuthException
{
    private const string ExceptionMessage =
        "The grant type specified is invalid.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidGrantTypeException"/>.
    /// </summary>
    public AuthInvalidGrantTypeException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidGrantTypeException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidGrantTypeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
