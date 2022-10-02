using System;
using System.Net;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// The base exception for all firebase exception.
/// </summary>
public class FirebaseException : ArgumentException
{
    /// <summary>
    /// Gets the request URL of the http operation.
    /// </summary>
    public string? RequestUrl { get; }

    /// <summary>
    /// Gets the content request of the http operation.
    /// </summary>
    public string? RequestContent { get; }

    /// <summary>
    /// Gets the content response of the http operation.
    /// </summary>
    public string? Response { get; }

    /// <summary>
    /// Gets the status code of the exception occured.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    internal FirebaseException(string message, string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception? innerException)
        : base(message, innerException)
    {
        RequestUrl = requestUrl;
        RequestContent = requestContent;
        Response = response;
        StatusCode = httpStatusCode;
    }
}
