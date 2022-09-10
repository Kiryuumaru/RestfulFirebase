using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the specified Realtime Database was not found.
/// </summary>
public class FirestoreDatabaseNotFoundException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "The specified Realtime Database was not found.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseNotFoundException"/>.
    /// </summary>
    public FirestoreDatabaseNotFoundException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseNotFoundException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseNotFoundException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
