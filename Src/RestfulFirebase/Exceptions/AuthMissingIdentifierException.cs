using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when an identifier was expected but one was not provided.
/// </summary>
public class AuthMissingIdentifierException : AuthException
{
    private const string ExceptionMessage =
        "Identifier was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingIdentifierException"/>.
    /// </summary>
    public AuthMissingIdentifierException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingIdentifierException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingIdentifierException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
