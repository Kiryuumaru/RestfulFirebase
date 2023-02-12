using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.Storage.Enums;
using System;
using System.Net;

namespace RestfulFirebase.Storage.Exceptions;

/// <summary>
/// Occurs when there`s an error in firestore storage.
/// </summary>
public class StorageException : FirebaseException
{
    /// <summary>
    /// Gets the <see cref="StorageErrorType"/> of the exception.
    /// </summary>
    public StorageErrorType ErrorType { get; }

    internal StorageException(StorageErrorType errorType, string message, string? requestUrl, string? requestContent, string? response, HttpStatusCode? httpStatusCode, Exception? innerException)
        : base(message, requestUrl, requestContent, response, httpStatusCode, innerException)
    {
        ErrorType = errorType;
    }
}
