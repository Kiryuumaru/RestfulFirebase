using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the supported provider identifier string is not a valid providerId.
/// </summary>
public class AuthInvalidProviderIDException : AuthException
{
    private const string ExceptionMessage =
        "The providerId must be a valid supported provider identifier string.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidProviderIDException"/>.
    /// </summary>
    public AuthInvalidProviderIDException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidProviderIDException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidProviderIDException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
