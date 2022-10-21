using System.Net.Http;

namespace RestfulFirebase;

public partial class FirebaseApp
{
    internal HttpClient GetClient()
    {
        return Config.HttpClientFactory?.GetHttpClient() ?? new HttpClient();
    }
}
