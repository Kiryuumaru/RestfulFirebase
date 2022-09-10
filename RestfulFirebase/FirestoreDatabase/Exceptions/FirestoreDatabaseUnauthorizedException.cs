using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by database rules.
/// </summary>
public class FirestoreDatabaseUnauthorizedException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "The request is not authorized by database rules.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseUnauthorizedException"/>.
    /// </summary>
    public FirestoreDatabaseUnauthorizedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseUnauthorizedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabaseUnauthorizedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
