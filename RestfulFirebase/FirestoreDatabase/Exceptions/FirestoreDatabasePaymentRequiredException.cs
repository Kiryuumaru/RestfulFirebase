using System;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request exceeds the database plan limits.
/// </summary>
public class FirestoreDatabasePaymentRequiredException : FirestoreDatabaseException
{
    private const string ExceptionMessage =
        "The request exceeds the database plan limits.";

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabasePaymentRequiredException"/>.
    /// </summary>
    public FirestoreDatabasePaymentRequiredException()
        : base(ExceptionMessage)
    {

    }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabasePaymentRequiredException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public FirestoreDatabasePaymentRequiredException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {

    }
}
