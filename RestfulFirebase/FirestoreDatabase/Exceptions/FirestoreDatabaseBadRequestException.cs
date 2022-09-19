using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request is malformed.
/// </summary>
public class FirestoreDatabaseBadRequestException : FirestoreDatabaseException
{
    internal FirestoreDatabaseBadRequestException(string? message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base(message ?? "Bad request.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
