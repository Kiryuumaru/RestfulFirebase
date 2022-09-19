using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when either the user or API keys are incorrect, or the API key has expired.
/// </summary>
public class AuthInvalidAccessTokenException : AuthException
{
    private const string ExceptionMessage =
        "Either the user or API keys are incorrect, or the API key has expired.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidAccessTokenException"/>.
    /// </summary>
    public AuthInvalidAccessTokenException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidAccessTokenException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidAccessTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
