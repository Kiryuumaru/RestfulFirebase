using System;
using System.Net;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class DatabaseUndefinedException : DatabaseException
{
    /// <summary>
    /// Gets the status code of the exception occured.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    internal DatabaseUndefinedException(Exception innerException, HttpStatusCode statusCode)
        : base("An unidentified error occured.", innerException)
    {
        StatusCode = statusCode;
    }
}
