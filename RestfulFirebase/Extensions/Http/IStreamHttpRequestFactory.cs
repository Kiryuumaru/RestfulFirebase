using System;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    public interface IStreamHttpRequestFactory
    {
        HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url);
    }
}
