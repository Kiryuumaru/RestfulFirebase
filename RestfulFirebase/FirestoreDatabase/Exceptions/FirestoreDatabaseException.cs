using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an error in firestore database.
/// </summary>
public class FirestoreDatabaseException : FirebaseException
{
    /// <summary>
    /// Gets the <see cref="FirestoreErrorType"/> of the exception.
    /// </summary>
    public FirestoreErrorType ErrorType { get; }

    internal FirestoreDatabaseException(FirestoreErrorType errorType, string message, string? requestUrl, string? requestContent, string? response, HttpStatusCode? httpStatusCode, Exception? innerException)
        : base(message, requestUrl, requestContent, response, httpStatusCode, innerException)
    {
        ErrorType = errorType;
    }
}
