using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.CloudFirestore.Query;

public abstract class Query
{
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
    /// </summary>
    public bool AuthenticateRequests { get; set; } = true;

    public Query(RestfulFirebaseApp app)
    {
        App = app;
    }

    internal async Task<HttpClient> GetClient()
    {
        var client = App.Config.CachedHttpClientFactory.GetHttpClient();

        if (AuthenticateRequests && App.Auth.Session != null)
        {
            string token = await App.Auth.Session.GetFreshToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    internal abstract string BuildUrl();

    internal abstract string BuildUrlSegment();
}
