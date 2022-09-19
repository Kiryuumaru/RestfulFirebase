using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request exceeds the database plan limits.
/// </summary>
public class FirestoreDatabasePaymentRequiredException : FirestoreDatabaseException
{
    internal FirestoreDatabasePaymentRequiredException(string? message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base(message ?? "The request exceeds the database plan limits.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
