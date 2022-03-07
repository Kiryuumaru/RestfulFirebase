using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the user account has been disabled by an administrator.
/// </summary>
public class AuthUserDisabledException : AuthException
{
    private const string ExceptionMessage =
        "The user account has been disabled by an administrator.";

    /// <summary>
    /// Creates an instance of <see cref="AuthUserDisabledException"/>.
    /// </summary>
    public AuthUserDisabledException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthUserDisabledException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthUserDisabledException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
