using RestfulFirebase.Common.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Requests;

/// <summary>
/// The base implementation for all firebase request.
/// </summary>
public abstract class TransactionRequest : IRequest
{
    /// <summary>
    /// Gets or sets the config of the request.
    /// </summary>
    public FirebaseConfig? Config { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Net.Http.HttpClient"/> used for the request.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Threading.CancellationToken"/> of the request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    internal abstract Task<HttpClient> GetClient();

    internal abstract Task<Exception> GetHttpException(HttpRequestMessage? request, HttpResponseMessage? response, HttpStatusCode httpStatusCode, Exception exception);
}

/// <summary>
/// The base implementation for all firebase request.
/// </summary>
/// <typeparam name="TResponse">
/// The type of the response.
/// </typeparam>
public abstract class TransactionRequest<TResponse> : TransactionRequest
{
    internal abstract Task<TResponse> Execute();

    internal async Task<(HttpResponseMessage? response, Exception? exception)> Execute(HttpMethod httpMethod, string uri)
    {
        ArgumentNullException.ThrowIfNull(Config);

        HttpClient httpClient = await GetClient();

        HttpRequestMessage request = new(httpMethod, uri);
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            response = await httpClient.SendAsync(request, CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return (response, null);
        }
        catch (OperationCanceledException ex)
        {
            return (null, ex);
        }
        catch (Exception ex)
        {
            return (null, await GetHttpException(request, response, statusCode, ex));
        }
    }

    internal async Task<(HttpResponseMessage? response, Exception? exception)> ExecuteWithContent(Stream contentStream, HttpMethod httpMethod, string uri)
    {
        ArgumentNullException.ThrowIfNull(Config);

        HttpClient httpClient = await GetClient();

        contentStream.Seek(0, SeekOrigin.Begin);

        StreamContent streamContent = new(contentStream);
        streamContent.Headers.ContentType = new("Application/json")
        {
            CharSet = Encoding.UTF8.WebName
        };
        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = streamContent
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            response = await httpClient.SendAsync(request, CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return (response, null);
        }
        catch (OperationCanceledException ex)
        {
            return (null, ex);
        }
        catch (Exception ex)
        {
            return (null, await GetHttpException(request, response, statusCode, ex));
        }
    }

    internal async Task<(HttpResponseMessage? response, Exception? exception)> ExecuteWithContent(string content, HttpMethod httpMethod, string uri)
    {
        ArgumentNullException.ThrowIfNull(Config);

        HttpClient httpClient = await GetClient();

        HttpRequestMessage request = new(httpMethod, uri)
        {
            Content = new StringContent(content, Encoding.UTF8, "Application/json")
        };
        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;

        try
        {
            response = await httpClient.SendAsync(request, CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return (response, null);
        }
        catch (OperationCanceledException ex)
        {
            return (null, ex);
        }
        catch (Exception ex)
        {
            return (null, await GetHttpException(request, response, statusCode, ex));
        }
    }
}

/// <summary>
/// The responses for all API request.
/// </summary>
public abstract class TransactionResponse
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

    internal TransactionResponse(Exception? error)
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

    internal TransactionResponse(TRequest request, TResult? response, Exception? error)
        : base(request, error)
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
            throw new NullReferenceException($"Response has no error but {nameof(Result)} is a null reference.");
        }
    }
}