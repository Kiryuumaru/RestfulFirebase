using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when an specified operation is disabled for this project.
/// </summary>
public class AuthOperationNotAllowedException : AuthException
{
    private const string ExceptionMessage =
        "Specified operation is disabled for this project.";

    internal AuthOperationNotAllowedException()
        : base(ExceptionMessage)
    {

    }

    internal AuthOperationNotAllowedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
