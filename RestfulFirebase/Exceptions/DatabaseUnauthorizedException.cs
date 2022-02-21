using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by database rules.
/// </summary>
public class DatabaseUnauthorizedException : DatabaseException
{
    private const string ExceptionMessage =
        "The request is not authorized by database rules.";

    internal DatabaseUnauthorizedException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseUnauthorizedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
