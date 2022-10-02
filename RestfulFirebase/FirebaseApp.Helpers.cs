using RestfulFirebase.Authentication;
using System.IO;
using System.Net;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase;

public partial class FirebaseApp
{
    internal HttpClient GetClient()
    {
        return Config.HttpClientFactory?.GetHttpClient() ?? new HttpClient();
    }
}
