using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    internal sealed class DefaultHttpClientProxy : IHttpClientProxy
    {
        private readonly HttpClient _httpClient;

        public DefaultHttpClientProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
