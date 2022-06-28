using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using RestfulFirebase.Exceptions;
using System.Threading.Tasks;
using RestfulFirebase.CloudFirestore.Query;
using DisposableHelpers;
using LockerHelpers;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RestfulFirebase.CloudFirestore;

/// <summary>
/// App module that provides firebase cloud firestore database implementations.
/// </summary>
public class CloudFirestoreApp : Disposable
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; private set; }

    internal const string CloudFirestoreDocumentsEndpoint = "https://firestore.googleapis.com/v1/projects/{0}/databases/{1}/documents/{2}";

    internal const string DefaultDatabase = "(default)";
    internal const string OfflineDatabaseIndicator = "cfdb";

    #endregion

    #region Initializers

    internal CloudFirestoreApp(RestfulFirebaseApp app)
    {
        App = app;
    }

    #endregion

    #region Methods

    public CollectionQuery Collection(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return new CollectionQuery(App, null, name);
    }

    #endregion

    #region Disposable Members



    #endregion
}
