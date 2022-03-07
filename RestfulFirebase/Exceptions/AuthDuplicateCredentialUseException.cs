using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the API request was received with a repeated or lower than expected nonce value.
/// </summary>
public class AuthDuplicateCredentialUseException : AuthException
{
    private const string ExceptionMessage =
        "API request was received with a repeated or lower than expected nonce value.";


    /// <summary>
    /// Creates an instance of <see cref="AuthDuplicateCredentialUseException"/>.
    /// </summary>
    public AuthDuplicateCredentialUseException()
        : base(ExceptionMessage)
    {

    }


    /// <summary>
    /// Creates an instance of <see cref="AuthDuplicateCredentialUseException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthDuplicateCredentialUseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
