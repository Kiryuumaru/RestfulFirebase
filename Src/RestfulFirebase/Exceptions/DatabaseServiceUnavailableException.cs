using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
/// </summary>
public class DatabaseServiceUnavailableException : DatabaseException
{
    private const string ExceptionMessage =
        "The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseServiceUnavailableException"/>.
    /// </summary>
    public DatabaseServiceUnavailableException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseServiceUnavailableException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseServiceUnavailableException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
