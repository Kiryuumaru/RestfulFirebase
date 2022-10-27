using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.RealtimeDatabase.Enums;
using System;
using System.Net;

namespace RestfulFirebase.RealtimeDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an error in firestore database.
/// </summary>
public class RealtimeDatabaseException : FirebaseException
{
    /// <summary>
    /// Gets the <see cref="RealtimeDatabaseErrorType"/> of the exception.
    /// </summary>
    public RealtimeDatabaseErrorType ErrorType { get; }

    internal RealtimeDatabaseException(RealtimeDatabaseErrorType errorType, string message, string? requestUrl, string? requestContent, string? response, HttpStatusCode? httpStatusCode, Exception? innerException)
        : base(message, requestUrl, requestContent, response, httpStatusCode, innerException)
    {
        ErrorType = errorType;
    }
}
