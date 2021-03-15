using System;
using System.Net.Http;

namespace RestfulFirebase
{
    public interface IHttpClientProxy : IDisposable
    {
        HttpClient GetHttpClient();
    }
}
