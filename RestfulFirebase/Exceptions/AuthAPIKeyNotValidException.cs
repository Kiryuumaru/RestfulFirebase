using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the provided API key is not valid.
/// </summary>
public class AuthAPIKeyNotValidException : AuthException
{
    private const string ExceptionMessage =
        "API key is not valid. Please pass a valid API key.";

    internal AuthAPIKeyNotValidException()
        : base(ExceptionMessage)
    {

    }

    internal AuthAPIKeyNotValidException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
