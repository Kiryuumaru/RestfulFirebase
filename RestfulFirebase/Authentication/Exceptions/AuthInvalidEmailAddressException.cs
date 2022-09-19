using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the email address is badly formatted.
/// </summary>
public class AuthInvalidEmailAddressException : AuthException
{
    private const string ExceptionMessage =
        "The email address is badly formatted.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidEmailAddressException"/>.
    /// </summary>
    public AuthInvalidEmailAddressException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidEmailAddressException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidEmailAddressException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
