using System;

namespace RestfulFirebase
{
    public interface IHttpClientFactory
    {
        IHttpClientProxy GetHttpClient(TimeSpan? timeout);
    }
}
