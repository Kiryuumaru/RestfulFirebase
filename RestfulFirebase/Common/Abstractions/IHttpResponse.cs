using System.Diagnostics.CodeAnalysis;
using System.Net;
using System;
using System.Net.Http;
using System.Threading;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The interface for all HTTP responses.
/// </summary>
public interface IHttpResponse
{
    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpRequestMessage"/> of the request.
    /// </summary>
    public HttpRequestMessage HttpRequestMessage { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpResponseMessage"/> of the request.
    /// </summary>
    public HttpResponseMessage? HttpResponseMessage { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.HttpStatusCode"/> of the request.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>
    /// Gets the exception of the operation.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(HttpResponseMessage))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(HttpResponseMessage))]
    public bool IsError { get; }

    void ThrowIfError();
}
