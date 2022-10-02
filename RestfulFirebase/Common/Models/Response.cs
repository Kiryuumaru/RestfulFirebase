using RestfulFirebase.Common.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Models;

/// <summary>
/// The responses for all API request.
/// </summary>
public abstract class Response
{
    /// <summary>
    /// Gets the exception of the operation.
    /// </summary>
    public virtual Exception? Error { get; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public virtual bool IsSuccess { get => Error == null; }

    /// <summary>
    /// Gets <c>true</c> whether the operation is successful; otherwise, <c>false</c>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public virtual bool IsError { get => Error != null; }

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
    public virtual TResult? Result { get; }

    /// <inheritdoc/>
    public override Exception? Error => base.Error;

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Result))]
    public override bool IsSuccess => base.IsSuccess;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Result))]
    public override bool IsError => base.IsError;

    internal Response(TResult? response, Exception? error)
        : base(error)
    {
        if ((response == null && error == null) ||
            (response != null && error != null))
        {
            throw new ArgumentException($"\"{nameof(response)}\" and \"{nameof(error)}\" cannot be both null or non-null");
        }

        Result = response;
    }

    /// <inheritdoc/>
    [MemberNotNull(nameof(Result))]
    public override void ThrowIfError()
    {
        if (Result == null)
        {
            throw Error!;
        }
    }
}