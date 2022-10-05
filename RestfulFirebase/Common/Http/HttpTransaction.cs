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
public class HttpTransaction
{
    /// <summary>
    /// Gets the request URL of the response.
    /// </summary>
    public string? RequestUrl { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpRequestMessage"/> of the request.
    /// </summary>
    public HttpRequestMessage HttpRequestMessage { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpResponseMessage"/> of the request.
    /// </summary>
    public HttpResponseMessage HttpResponseMessage { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.HttpStatusCode"/> of the request.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    internal HttpTransaction(HttpRequestMessage request, HttpResponseMessage response, HttpStatusCode httpStatusCode)
    {
        HttpRequestMessage = request;
        HttpResponseMessage = response;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// Gets the <see cref="HttpRequestMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetRequestContentAsString()
    {
        if (HttpRequestMessage?.Content == null)
        {
            return null;
        }

        return await HttpRequestMessage.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets the <see cref="HttpResponseMessage.Content"/> as string.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the string content.
    /// </returns>
    public async Task<string?> GetResponseContentAsString()
    {
        if (HttpResponseMessage?.Content == null)
        {
            return null;
        }

        return await HttpResponseMessage.Content.ReadAsStringAsync();
    }
}
