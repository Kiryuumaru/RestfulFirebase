using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there is no user record corresponding to the identifier. The user may have been deleted.
/// </summary>
public class AuthEmailNotFoundException : AuthException
{
    private const string ExceptionMessage =
        "There is no user record corresponding to this identifier. The user may have been deleted.";

    internal AuthEmailNotFoundException()
        : base(ExceptionMessage)
    {

    }

    internal AuthEmailNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
