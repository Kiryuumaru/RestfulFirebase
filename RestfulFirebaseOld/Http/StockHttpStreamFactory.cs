﻿using System.Net;
using System.Net.Http;

namespace RestfulFirebase.Http;

/// <summary>
/// The provided stock <see cref="IHttpStreamFactory"/> implementation to be used.
/// </summary>
public sealed class StockHttpStreamFactory : IHttpStreamFactory
{
    /// <summary>
    /// Creates new instance of <see cref="StockHttpStreamFactory"/> class.
    /// </summary>
    public StockHttpStreamFactory()
    {

    }

    /// <inheritdoc/>
    public HttpClient GetHttpClient()
    {
        return new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            CookieContainer = new CookieContainer()
        }, true);
    }

    /// <inheritdoc/>
    public HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url)
    {
        return new HttpRequestMessage(method, url);
    }
}
