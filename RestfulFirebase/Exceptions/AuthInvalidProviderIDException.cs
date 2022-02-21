using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the supported provider identifier string is not a valid providerId.
/// </summary>
public class AuthInvalidProviderIDException : AuthException
{
    private const string ExceptionMessage =
        "The providerId must be a valid supported provider identifier string.";

    internal AuthInvalidProviderIDException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidProviderIDException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
