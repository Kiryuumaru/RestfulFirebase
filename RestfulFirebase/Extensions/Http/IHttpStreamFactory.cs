using System;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    public interface IHttpStreamFactory
    {
        HttpClient GetHttpClient();
        HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url);
    }
}
