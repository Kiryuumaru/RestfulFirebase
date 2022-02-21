using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is malformed.
/// </summary>
public class DatabaseBadRequestException : DatabaseException
{
    private const string ExceptionMessage =
        "Bad request.";

    internal DatabaseBadRequestException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseBadRequestException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
