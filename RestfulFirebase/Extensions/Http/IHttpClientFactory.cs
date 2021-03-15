using System;

namespace RestfulFirebase.Extensions.Http
{
    public interface IHttpClientFactory
    {
        IHttpClientProxy GetHttpClient(TimeSpan? timeout);
    }
}
