using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the reset password request exceeds its limit.
/// </summary>
public class AuthResetPasswordExceedLimitException : AuthException
{
    private const string ExceptionMessage =
        "The reset password request exceeds its limit.";

    /// <summary>
    /// Creates an instance of <see cref="AuthResetPasswordExceedLimitException"/>.
    /// </summary>
    public AuthResetPasswordExceedLimitException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthResetPasswordExceedLimitException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthResetPasswordExceedLimitException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
