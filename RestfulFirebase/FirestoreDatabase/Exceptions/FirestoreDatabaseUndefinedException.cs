using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an unidentified exception.
/// </summary>
public class FirestoreDatabaseUndefinedException : FirestoreDatabaseException
{
    /// <summary>
    /// Gets the status code of the exception occured.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Creates an instance of <see cref="FirestoreDatabaseUndefinedException"/> with provided <paramref name="innerException"/> and <paramref name="statusCode"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    /// <param name="statusCode">
    /// The <see cref="HttpStatusCode"/> of the exception.
    /// </param>
    public FirestoreDatabaseUndefinedException(Exception innerException, HttpStatusCode statusCode)
        : base("An unidentified error occured.", innerException)
    {
        StatusCode = statusCode;
    }
}
