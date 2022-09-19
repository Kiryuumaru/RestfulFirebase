using System;
using System.Net;
using System.Net.Http;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class FirestoreDatabaseUndefinedException : FirestoreDatabaseException
{
    internal FirestoreDatabaseUndefinedException(string? message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base(message ?? "An unidentified error occured.", requestUrl, requestContent, response, httpStatusCode, exception)
    {
    }
}
