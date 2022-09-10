using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an error in realtime database.
/// </summary>
public class FirestoreDatabaseException : Exception
{
    private const string ExceptionMessage =
        "An realtime database error occured.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseException"/>.
    /// </summary>
    public FirestoreDatabaseException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseException"/> with provided <paramref name="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    public FirestoreDatabaseException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseException"/> with provided <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
