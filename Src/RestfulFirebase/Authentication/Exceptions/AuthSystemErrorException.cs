using System;

namespace RestfulFirebase.Authentication.Exceptions;

/// <summary>
/// Occurs when the system has error.
/// </summary>
public class AuthSystemErrorException : AuthException
{
    private const string ExceptionMessage =
        "A system error has occurred.";

    /// <summary>
    /// Creates an instance of <see cref="AuthSystemErrorException"/>.
    /// </summary>
    public AuthSystemErrorException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthSystemErrorException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthSystemErrorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
