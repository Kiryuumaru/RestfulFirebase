using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the action code has expired.
/// </summary>
public class AuthExpiredOOBCodeException : AuthException
{
    private const string ExceptionMessage =
        "The action code has expired.";

    /// <summary>
    /// Creates an instance of <see cref="AuthExpiredOOBCodeException"/>.
    /// </summary>
    public AuthExpiredOOBCodeException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthExpiredOOBCodeException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthExpiredOOBCodeException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
