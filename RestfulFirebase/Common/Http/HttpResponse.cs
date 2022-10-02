using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Http;

/// <summary>
/// The base response for all HTTP requests.
/// </summary>
public class HttpResponse : Response, IHttpResponse
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

    internal HttpResponse(HttpRequestMessage request, HttpResponseMessage? response, HttpStatusCode httpStatusCode, Exception? error)
        : base(error)
    {
        HttpRequestMessage = request;
        HttpResponseMessage = response;
        HttpStatusCode = httpStatusCode;
    }

    internal HttpResponse(IHttpResponse response)
        : base(response.Error)
    {
        HttpRequestMessage = response.HttpRequestMessage;
        HttpResponseMessage = response.HttpResponseMessage;
        HttpStatusCode = response.HttpStatusCode;
    }

    /// <summary>
    /// Gets the <see cref="HttpRequestMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetRequestContentAsString()
    {
        if (HttpRequestMessage.Content == null)
        {
            return null;
        }

        return HttpRequestMessage?.Content == null ? null : await HttpRequestMessage.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets the <see cref="HttpResponseMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetResponseContentAsString()
    {
        return HttpResponseMessage?.Content == null ? null : await HttpResponseMessage.Content.ReadAsStringAsync();
    }
}

/// <summary>
/// The base response for all HTTP requests.
/// </summary>
/// <inheritdoc/>
public class HttpResponse<TResult> : Response<TResult>, IHttpResponse
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

    /// <inheritdoc/>
    public override TResult? Result => base.Result;

    /// <inheritdoc/>
    public override Exception? Error => base.Error;

    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(true, nameof(HttpResponseMessage))]
    public override bool IsSuccess => base.IsSuccess;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Result))]
    [MemberNotNullWhen(false, nameof(HttpResponseMessage))]
    public override bool IsError => base.IsError;

    internal HttpResponse(HttpRequestMessage request, HttpResponseMessage? response, TResult? result, HttpStatusCode httpStatusCode, Exception? error)
        : base(result, error)
    {
        HttpRequestMessage = request;
        HttpResponseMessage = response;
        HttpStatusCode = httpStatusCode;
    }

    internal HttpResponse(TResult? result, IHttpResponse response)
        : base(result, response.Error)
    {
        HttpRequestMessage = response.HttpRequestMessage;
        HttpResponseMessage = response.HttpResponseMessage;
        HttpStatusCode = response.HttpStatusCode;
    }

    /// <inheritdoc/>
    [MemberNotNull(nameof(Result))]
    public override void ThrowIfError()
    {
        if (base.Result == null || Result == null)
        {
            throw Error!;
        }
    }

    /// <summary>
    /// Gets the <see cref="HttpRequestMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetRequestContentAsString()
    {
        if (HttpRequestMessage.Content == null)
        {
            return null;
        }

        return HttpRequestMessage?.Content == null ? null : await HttpRequestMessage.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets the <see cref="HttpResponseMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetResponseContentAsString()
    {
        return HttpResponseMessage?.Content == null ? null : await HttpResponseMessage.Content.ReadAsStringAsync();
    }
}
