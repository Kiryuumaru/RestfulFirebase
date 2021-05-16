using System;
using System.Net;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    public class DefaultHttpClientHandler : IHttpClientHandlerFactory
    {
        public HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
                CookieContainer = new CookieContainer()
            };
        }
    }
}
