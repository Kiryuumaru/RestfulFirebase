using ObservableHelpers;
using ObservableHelpers.Utilities;
using SynchronizationContextHelpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RestfulFirebase.Storage;

/// <summary>
/// App module that provides firebase storage implementations
/// </summary>
public class StorageApp : SyncContext
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    #endregion

    #region Initializers

    internal StorageApp(RestfulFirebaseApp app)
    {
        SyncOperation.SetContext(app);

        App = app;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates new instance of <see cref="StorageBucket"/> reference.
    /// </summary>
    /// <param name="bucket">
    /// The storage bucket (i.e., "projectid.appspot.com").
    /// </param>
    /// <returns>
    /// The instance of <see cref="StorageBucket"/> reference.
    /// </returns>
    public StorageBucket Bucket(string bucket)
    {
        return new StorageBucket(App, bucket);
    }

    internal HttpClient CreateHttpClientAsync()
    {
        var client = App.Config.CachedHttpClientFactory.GetHttpClient();

        if (App.Auth.Session != null)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", App.Auth.Session.FirebaseToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Auth.Session.FirebaseToken);
        }

        return client;
    }

    #endregion
}
