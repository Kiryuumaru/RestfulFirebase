using System;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Common.Transactions;

/// <summary>
/// The responses for all API request.
/// </summary>
public abstract class TransactionResponse
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

    internal TransactionResponse(Exception? error)
    {
        Error = error;
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
public class TransactionResponse<TRequest> : TransactionResponse
    where TRequest : TransactionRequest
{
    /// <summary>
    /// Gets the <typeparamref name="TRequest"/> of the operation.
    /// </summary>
    public TRequest Request { get; }

    internal TransactionResponse(TRequest request, Exception? error)
        : base(error)
    {
        Request = request;
    }
}

/// <summary>
/// The responses for all API request.
/// </summary>
/// <typeparam name="TResult">
/// The type of the operation response.
/// </typeparam>
/// <typeparam name="TRequest">
/// The type of the operation request.
/// </typeparam>
public class TransactionResponse<TRequest, TResult> : TransactionResponse<TRequest>
    where TRequest : TransactionRequest
{
    /// <summary>
    /// Gets the <typeparamref name="TResult"/> of the operation.
    /// </summary>
    public TResult? Result { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    public bool HasResult { get => Result != null; }

    internal TransactionResponse(TRequest request, TResult? response, Exception? error)
        : base(request, error)
    {
        Result = response;
    }

    /// <summary>
    /// Throws if the response is empty or has any error.
    /// </summary>
    [MemberNotNull(nameof(Result))]
    public void ThrowIfEmptyResult()
    {
        if (Result == null)
        {
            throw new NullReferenceException($"{nameof(Result)} is a null reference.");
        }
    }

    /// <summary>
    /// Throws if the response is empty or has any error.
    /// </summary>
    [MemberNotNull(nameof(Result))]
    public void ThrowIfErrorOrEmptyResult()
    {
        if (Error != null)
        {
            throw Error;
        }
        else if (Result == null)
        {
            throw new NullReferenceException($"Response has no error but {nameof(Result)} is a null reference.");
        }
    }
}