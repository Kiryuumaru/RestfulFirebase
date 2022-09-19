using System;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when an specified operation is disabled for this project.
/// </summary>
public class AuthOperationNotAllowedException : AuthException
{
    private const string ExceptionMessage =
        "Specified operation is disabled for this project.";

    /// <summary>
    /// Creates an instance of <see cref="AuthOperationNotAllowedException"/>.
    /// </summary>
    public AuthOperationNotAllowedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="AuthOperationNotAllowedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public AuthOperationNotAllowedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
