using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified credential is already associated with a different user account.
/// </summary>
public class AuthAlreadyLinkedException : AuthException
{
    private const string ExceptionMessage =
        "This credential is already associated with a different user account.";

    internal AuthAlreadyLinkedException()
        : base(ExceptionMessage)
    {

    }

    internal AuthAlreadyLinkedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
