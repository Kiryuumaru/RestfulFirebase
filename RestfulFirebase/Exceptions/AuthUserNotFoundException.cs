using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
/// </summary>
public class AuthUserNotFoundException : AuthException
{
    private const string ExceptionMessage =
        "There is no user record corresponding to this identifier. The user may have been deleted.";

    internal AuthUserNotFoundException()
        : base(ExceptionMessage)
    {

    }

    internal AuthUserNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
