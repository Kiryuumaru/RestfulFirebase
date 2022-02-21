using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class StorageUndefinedException : StorageException
{
    private const string ExceptionMessage =
        "An unidentified error occured.";

    internal StorageUndefinedException()
        : base(ExceptionMessage)
    {

    }

    internal StorageUndefinedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
