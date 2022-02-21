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
    public IHttpClientProxy GetHttpClient(TimeSpan? timeout)
    {
        var client = new HttpClient();
        if (timeout != null) {
            client.Timeout = timeout.Value;
        }

        return new StockHttpClientProxy(client);
    }
}
