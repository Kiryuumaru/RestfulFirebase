using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
/// </summary>
public class FirestoreDatabasePreconditionFailedException : FirestoreDatabaseException
{
    internal FirestoreDatabasePreconditionFailedException(string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base("The request's specified ETag value in the if-match header did not match the server's value.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
