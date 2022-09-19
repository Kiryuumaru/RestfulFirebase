using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the email address is already in use by another account.
/// </summary>
public class AuthEmailExistsException : AuthException
{
    private const string ExceptionMessage =
        "The email address is already in use by another account.";

    /// <summary>
    /// Creates an instance of <see cref="AuthEmailExistsException"/>.
    /// </summary>
    public AuthEmailExistsException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthEmailExistsException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthEmailExistsException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
