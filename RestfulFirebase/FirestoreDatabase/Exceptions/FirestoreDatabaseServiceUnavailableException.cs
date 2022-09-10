using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
/// </summary>
public class FirestoreDatabaseServiceUnavailableException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseServiceUnavailableException"/>.
    /// </summary>
    public FirestoreDatabaseServiceUnavailableException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseServiceUnavailableException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseServiceUnavailableException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
