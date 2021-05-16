using System.Net;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    internal sealed class DefaultHttpStreamFactory : IHttpStreamFactory
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
}
