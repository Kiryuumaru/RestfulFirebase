using RestfulFirebase.Http;
using System.Net.Http;

namespace RestfulFirebase.Common.Models;

internal sealed class HttpClientFactory : IHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        return new HttpClient();
    }
}
