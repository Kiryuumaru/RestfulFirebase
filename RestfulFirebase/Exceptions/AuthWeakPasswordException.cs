using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the password is less than 6 characters long.
/// </summary>
public class AuthWeakPasswordException : AuthException
{
    private const string ExceptionMessage =
        "The password must be 6 characters long or more.";

    /// <summary>
    /// Creates an instance of <see cref="AuthWeakPasswordException"/>.
    /// </summary>
    public AuthWeakPasswordException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthWeakPasswordException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthWeakPasswordException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
