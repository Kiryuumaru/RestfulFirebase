using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    internal sealed class DefaultStreamHttpRequestFactory : IStreamHttpRequestFactory
    {
        public HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            return request;
        }
    }
}
