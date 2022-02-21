using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified Realtime Database was not found.
/// </summary>
public class DatabaseNotFoundException : DatabaseException
{
    private const string ExceptionMessage =
        "The specified Realtime Database was not found.";

    internal DatabaseNotFoundException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
