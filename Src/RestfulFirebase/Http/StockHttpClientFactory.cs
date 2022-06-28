using System;
using System.Net.Http;

namespace RestfulFirebase.Http;

/// <summary>
/// The provided stock <see cref="IHttpClientFactory"/> implementation to be used.
/// </summary>
public sealed class StockHttpClientFactory : IHttpClientFactory
{
    /// <summary>
    /// Creates new instance of <see cref="StockHttpClientFactory"/> class.
    /// </summary>
    public StockHttpClientFactory()
    {

    }

    /// <inheritdoc/>
    public HttpClient GetHttpClient()
    {
        return new HttpClient();
    }
}
