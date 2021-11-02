using ObservableHelpers;
using ObservableHelpers.Utilities;
using System.Net.Http;

namespace RestfulFirebase.Http
{
    /// <summary>
    /// The provided stock <see cref="IHttpClientProxy"/> implementation to be used.
    /// </summary>
    public sealed class StockHttpClientProxy : Disposable, IHttpClientProxy
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates new instance of <see cref="StockHttpClientProxy"/> class.
        /// </summary>
        /// <param name="httpClient">
        /// The http client to proxy.
        /// </param>
        public StockHttpClientProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
