using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the specified Realtime Database was not found.
/// </summary>
public class FirestoreDatabaseNotFoundException : FirestoreDatabaseException
{
    internal FirestoreDatabaseNotFoundException(string? message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base(message ?? "The specified Realtime Database was not found.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
