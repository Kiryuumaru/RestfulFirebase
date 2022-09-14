using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RestfulFirebase.Common.Responses;

/// <summary>
/// The responses for all API request.
/// </summary>
public abstract class CommonResponse
{
    /// <summary>
    /// Gets the exception of the operation.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get => Error == null; }

    internal CommonResponse(Exception? error)
    {
        Error = error;
    }

    internal static CommonResponse<TRequest> Create<TRequest>(TRequest request, Exception? error = null)
        where TRequest : CommonRequest
    {
        return new CommonResponse<TRequest>(request, error);
    }

    internal static CommonResponse<TRequest, TResponse> Create<TRequest, TResponse>(TRequest request, TResponse? response, Exception? error = null)
        where TRequest : CommonRequest
    {
        return new CommonResponse<TRequest, TResponse>(request, response, error);
    }

    /// <summary>
    /// Throws if the response has any error.
    /// </summary>
    public void ThrowIfError()
    {
        if (Error != null)
        {
            throw Error;
        }
    }
}

/// <summary>
/// The responses for all API request.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the operation request.
/// </typeparam>
public class CommonResponse<TRequest> : CommonResponse
    where TRequest : CommonRequest
{
    /// <summary>
    /// Gets the <typeparamref name="TRequest"/> of the operation.
    /// </summary>
    public TRequest Request { get; }

    internal CommonResponse(TRequest request, Exception? error)
        : base(error)
    {
        Request = request;
    }
}

/// <summary>
/// The responses for all API request.
/// </summary>
/// <typeparam name="TResponse">
/// The type of the operation response.
/// </typeparam>
/// <typeparam name="TRequest">
/// The type of the operation request.
/// </typeparam>
public class CommonResponse<TRequest, TResponse> : CommonResponse<TRequest>
    where TRequest : CommonRequest
{
    /// <summary>
    /// Gets the <typeparamref name="TResponse"/> of the operation.
    /// </summary>
    public TResponse? Response { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Response))]
    public bool HasResponse { get => Response != null; }

    internal CommonResponse(TRequest request, TResponse? response, Exception? error)
        : base(request, error)
    {
        Response = response;
    }

    /// <summary>
    /// Throws if the response is empty or has any error.
    /// </summary>
    [MemberNotNull(nameof(Response))]
    public void ThrowIfEmptyResponse()
    {
        if (Response == null)
        {
            throw new NullReferenceException($"{nameof(Response)} is a null reference.");
        }
    }

    /// <summary>
    /// Throws if the response is empty or has any error.
    /// </summary>
    [MemberNotNull(nameof(Response))]
    public void ThrowIfErrorOrEmptyResponse()
    {
        if (Error != null)
        {
            throw Error;
        }
        else if (Response == null)
        {
            throw new NullReferenceException($"Response has no error but {nameof(Response)} is a null reference.");
        }
    }
}
