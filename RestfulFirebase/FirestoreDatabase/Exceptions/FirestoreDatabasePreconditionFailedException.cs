using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
/// </summary>
public class FirestoreDatabasePreconditionFailedException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "The request's specified ETag value in the if-match header did not match the server's value.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabasePreconditionFailedException"/>.
    /// </summary>
    public FirestoreDatabasePreconditionFailedException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabasePreconditionFailedException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabasePreconditionFailedException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
