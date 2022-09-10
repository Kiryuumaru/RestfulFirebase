using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request is malformed.
/// </summary>
public class FirestoreDatabaseBadRequestException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "Bad request.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseBadRequestException"/>.
    /// </summary>
    public FirestoreDatabaseBadRequestException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseBadRequestException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseBadRequestException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
