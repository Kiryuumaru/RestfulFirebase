using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the API request was received with a repeated or lower than expected nonce value.
/// </summary>
public class AuthDuplicateCredentialUseException : AuthException
{
    private const string ExceptionMessage =
        "API request was received with a repeated or lower than expected nonce value.";

    internal AuthDuplicateCredentialUseException()
        : base(ExceptionMessage)
    {

    }

    internal AuthDuplicateCredentialUseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
