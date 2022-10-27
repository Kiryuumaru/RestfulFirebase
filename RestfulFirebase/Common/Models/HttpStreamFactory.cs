using RestfulFirebase.Http;
using System.Net;
using System.Net.Http;

namespace RestfulFirebase.Common.Models;

internal class HttpStreamFactory : IHttpStreamFactory
{
    public HttpClient GetHttpClient()
    {
        return new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            CookieContainer = new CookieContainer()
        }, true);
    }

    public HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url)
    {
        return new HttpRequestMessage(method, url);
    }
}
