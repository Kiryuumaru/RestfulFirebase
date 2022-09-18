using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
/// </summary>
public class FirestoreDatabaseServiceUnavailableException : FirestoreDatabaseException
{
    internal FirestoreDatabaseServiceUnavailableException(string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base("The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
