using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when password was expected but one was not provided.
/// </summary>
public class AuthMissingPasswordException : AuthException
{
    private const string ExceptionMessage =
        "Password was expected but one was not provided.";

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingPasswordException"/>.
    /// </summary>
    public AuthMissingPasswordException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthMissingPasswordException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthMissingPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
