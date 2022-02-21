using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by storage rules.
/// </summary>
public class StorageUnauthorizedException : StorageException
{
    private const string ExceptionMessage =
        "The request is not authorized by storage rules.";

    internal StorageUnauthorizedException()
        : base(ExceptionMessage)
    {

    }

    internal StorageUnauthorizedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
