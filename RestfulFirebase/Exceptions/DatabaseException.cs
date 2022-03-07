using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in realtime database.
/// </summary>
public class DatabaseException : Exception
{
    private const string ExceptionMessage =
        "An realtime database error occured.";

    /// <summary>
    /// Creates an instance of <see cref="DatabaseException"/>.
    /// </summary>
    public DatabaseException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseException"/> with provided <paramref name="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    public DatabaseException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="DatabaseException"/> with provided <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public DatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
