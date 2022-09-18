using System;
using System.Net;
using System.Net.Http;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class FirestoreDatabaseUndefinedException : FirestoreDatabaseException
{
    internal FirestoreDatabaseUndefinedException(string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base("An unidentified error occured.", requestUrl, requestContent, response, httpStatusCode, exception)
    {
    }
}
