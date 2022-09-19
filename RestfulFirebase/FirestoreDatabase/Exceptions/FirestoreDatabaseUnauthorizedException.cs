using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request is not authorized by database rules.
/// </summary>
public class FirestoreDatabaseUnauthorizedException : FirestoreDatabaseException
{
    internal FirestoreDatabaseUnauthorizedException(string? message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base(message ?? "The request is not authorized by database rules.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
