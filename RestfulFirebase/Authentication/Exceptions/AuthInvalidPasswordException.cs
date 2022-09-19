using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the password is invalid or the user does not have a password.
/// </summary>
public class AuthInvalidPasswordException : AuthException
{
    private const string ExceptionMessage =
        "The password is invalid or the user does not have a password.";

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidPasswordException"/>.
    /// </summary>
    public AuthInvalidPasswordException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthInvalidPasswordException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthInvalidPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
