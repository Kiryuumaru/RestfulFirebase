using RestfulFirebase.Common.Models;
using RestfulFirebase.Http;
using System.Net.Http;

namespace RestfulFirebase;

public partial class FirebaseApp
{
    internal static IHttpClientFactory DefaultHttpClientFactory = new HttpClientFactory();
    internal static IHttpStreamFactory DefaultHttpStreamFactory = new HttpStreamFactory();

    internal HttpClient GetHttpClient()
    {
        return Config.HttpClientFactory?.GetHttpClient() ?? DefaultHttpClientFactory.GetHttpClient();
    }

    internal HttpClient GetStreamHttpClient()
    {
        return Config.HttpStreamFactory?.GetHttpClient() ?? DefaultHttpStreamFactory.GetHttpClient();
    }

    internal HttpRequestMessage GetStreamHttpRequestMessage(HttpMethod httpMethod, string url)
    {
        return Config.HttpStreamFactory?.GetStreamHttpRequestMessage(httpMethod, url) ?? DefaultHttpStreamFactory.GetStreamHttpRequestMessage(httpMethod, url);
    }
}
