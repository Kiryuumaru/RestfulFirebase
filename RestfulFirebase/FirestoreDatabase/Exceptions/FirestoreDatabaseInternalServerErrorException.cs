using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an internal server error.
/// </summary>
public class FirestoreDatabaseInternalServerErrorException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "An internal server error occured.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseInternalServerErrorException"/>.
    /// </summary>
    public FirestoreDatabaseInternalServerErrorException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseInternalServerErrorException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseInternalServerErrorException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
