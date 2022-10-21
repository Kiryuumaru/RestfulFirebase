using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using RestfulFirebase.Common.Http;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The interface for all HTTP responses.
/// </summary>
public interface IHttpResponse
{
    /// <summary>
    /// Gets all http transactions made by the request.
    /// </summary>
    public IReadOnlyList<HttpTransaction> HttpTransactions { get; }

    /// <summary>
    /// Gets the exception of the operation.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError { get; }

    /// <summary>
    /// Throws if the response has any error.
    /// </summary>
    void ThrowIfError();
}
