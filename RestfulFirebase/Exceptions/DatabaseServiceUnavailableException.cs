using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
/// </summary>
public class DatabaseServiceUnavailableException : DatabaseException
{
    private const string ExceptionMessage =
        "The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.";

    internal DatabaseServiceUnavailableException()
        : base(ExceptionMessage)
    {

    }

    internal DatabaseServiceUnavailableException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
