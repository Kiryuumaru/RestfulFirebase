using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Query;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract class Query
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets the <see cref="FirestoreDatabase"/> used by this instance.
    /// </summary>
    public FirestoreDatabase Database { get; }

    /// <summary>
    /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
    /// </summary>
    public bool AuthenticateRequests { get; set; } = true;

    #endregion

    #region Initializers

    internal Query(RestfulFirebaseApp app, FirestoreDatabase firestoreDatabase)
    {
        App = app;
        Database = firestoreDatabase;
    }

    #endregion

    #region Methods

    internal async Task<HttpClient> GetClient()
    {
        var client = App.Config.HttpClientFactory.GetHttpClient();

        if (AuthenticateRequests && App.Auth.Session != null)
        {
            string token = await App.Auth.Session.GetFreshToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    internal abstract string BuildUrl();

    internal abstract string BuildUrlSegment();

    #endregion
}
