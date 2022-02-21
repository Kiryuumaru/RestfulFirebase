using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the reset password request exceeds its limit.
/// </summary>
public class AuthResetPasswordExceedLimitException : AuthException
{
    private const string ExceptionMessage =
        "The reset password request exceeds its limit.";

    internal AuthResetPasswordExceedLimitException()
        : base(ExceptionMessage)
    {

    }

    internal AuthResetPasswordExceedLimitException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
