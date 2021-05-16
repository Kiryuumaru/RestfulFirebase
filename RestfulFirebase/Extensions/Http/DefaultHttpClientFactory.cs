using System;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    internal sealed class DefaultHttpClientFactory : IHttpClientFactory
    {
        public IHttpClientProxy GetHttpClient(TimeSpan? timeout)
        {
            var client = new HttpClient();
            if (timeout != null) {
                client.Timeout = timeout.Value;
            }

            return new DefaultHttpClientProxy(client);
        }
    }
}
