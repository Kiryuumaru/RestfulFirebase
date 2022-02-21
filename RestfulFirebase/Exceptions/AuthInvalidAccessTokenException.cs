using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when either the user or API keys are incorrect, or the API key has expired.
/// </summary>
public class AuthInvalidAccessTokenException : AuthException
{
    private const string ExceptionMessage =
        "Either the user or API keys are incorrect, or the API key has expired.";

    internal AuthInvalidAccessTokenException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidAccessTokenException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
