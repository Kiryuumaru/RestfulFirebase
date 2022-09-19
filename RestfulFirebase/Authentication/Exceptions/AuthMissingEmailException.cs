using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when an email address was expected but one was not provided.
/// </summary>
public class AuthMissingEmailException : AuthException
{
    private const string ExceptionMessage =
        "Email address was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingEmailException"/>.
    /// </summary>
    public AuthMissingEmailException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingEmailException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingEmailException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
