using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RestfulFirebase.Common.Models;

/// <summary>
/// The responses for all API request.
/// </summary>
public class Response
{
    /// <summary>
    /// Gets the exception of the operation.
    /// </summary>
    public virtual Exception? Error { get; protected set; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public virtual bool IsSuccess { get => Error == null; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is unsuccessful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public virtual bool IsError { get => Error != null; }

    internal Response()
    {

    }

    internal Response(Exception? error)
    {
        Error = error;
    }

    /// <summary>
    /// Throws if the response has any error.
    /// </summary>
    public virtual void ThrowIfError()
    {
        if (Error != null)
        {
            throw Error;
        }
    }

    internal virtual void Append(params Response[] responses)
    {
        if (responses.LastOrDefault() is Response lastResponse)
        {
            Error = lastResponse.Error;
        }
    }

    internal virtual void Append(Exception? error)
    {
        Error = error;
    }
}

/// <summary>
/// The responses for all API request.
/// </summary>
/// <typeparam name="TResult">
/// The type of the operation response.
/// </typeparam>
public class Response<TResult> : Response
{
    /// <summary>
    /// Gets the <typeparamref name="TResult"/> of the operation.
    /// </summary>
    public virtual TResult? Result { get; protected set; }

    /// <inheritdoc/>
    public override Exception? Error { get => base.Error; protected set => base.Error = value; }

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Result))]
    public override bool IsSuccess => base.IsSuccess;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Result))]
    public override bool IsError => base.IsError;

    internal Response(TResult? response)
        : base(null)
    {
        Result = response;
    }

    internal Response(Exception? error)
        : base(error)
    {

    }

    internal Response(TResult? response, Exception? error)
        : base(error)
    {
        Result = response;
    }

    /// <inheritdoc/>
    [MemberNotNull(nameof(Result))]
    public override void ThrowIfError()
    {
        if (Error != null)
        {
            throw Error;
        }
        else if (Result == null)
        {
            throw new NullReferenceException($"Response has no \"{nameof(Result)}\".");
        }
    }

    internal override void Append(params Response[] responses)
    {
        if (responses.LastOrDefault() is Response lastResponse)
        {
            Error = lastResponse.Error;
            if (lastResponse is Response<TResult> lastTypedResponse)
            {
                Result = lastTypedResponse.Result;
            }
        }
    }

    internal virtual void Append(TResult? result)
    {
        Result = result;
    }
}