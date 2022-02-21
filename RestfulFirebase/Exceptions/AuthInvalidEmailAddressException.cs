using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the email address is badly formatted.
/// </summary>
public class AuthInvalidEmailAddressException : AuthException
{
    private const string ExceptionMessage =
        "The email address is badly formatted.";

    internal AuthInvalidEmailAddressException()
        : base(ExceptionMessage)
    {

    }

    internal AuthInvalidEmailAddressException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
