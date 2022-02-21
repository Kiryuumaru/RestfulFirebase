using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;
using ObservableHelpers.Utilities;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using RestfulFirebase.Exceptions;
using System.Threading.Tasks;
using RestfulFirebase.Database.Realtime;
using SynchronizationContextHelpers;
using DisposableHelpers;
using LockerHelpers;

namespace RestfulFirebase.Database;

/// <summary>
/// App module that provides firebase realtime database implementations.
/// </summary>
public class DatabaseApp : SyncContext
{
    #region Properties

    /// <inheritdoc/>
    public RestfulFirebaseApp App { get; private set; }

    internal const string OfflineDatabaseIndicator = "db";

    private readonly OperationInvoker writeTaskPutControl = new(0);
    private readonly ConcurrentDictionary<string[], WriteTask> writeTasks = new(PathEqualityComparer.Instance);

    #endregion

    #region Initializers

    internal DatabaseApp(RestfulFirebaseApp app)
    {
        SyncOperation.SetContext(app);

        App = app;

        App.Config.ImmediatePropertyChanged += Config_ImmediatePropertyChanged;

        writeTaskPutControl.ConcurrentTokenCount = App.Config.CachedDatabaseMaxConcurrentSyncWrites;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="resourceNameFactory"/>.
    /// </summary>
    /// <param name="resourceNameFactory">
    /// The resource name factory of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="ChildQuery"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="resourceNameFactory"/> or <see cref="FirebaseConfig.DatabaseURL"/> is null.
    /// </exception>
    public ChildQuery Child(Func<string> resourceNameFactory)
    {
        if (resourceNameFactory == null)
        {
            throw new ArgumentNullException(nameof(resourceNameFactory));
        }

        if (App.Config.CachedDatabaseURL == null)
        {
            throw new ArgumentNullException(nameof(App.Config.DatabaseURL));
        }

        return new ChildQuery(App, null, () => UrlUtilities.Combine(App.Config.CachedDatabaseURL, resourceNameFactory()));
    }

    /// <summary>
    /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="resourceName"/>.
    /// </summary>
    /// <param name="resourceName">
    /// The resource name of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="ChildQuery"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="resourceName"/> is null or empty.
    /// </exception>
    public ChildQuery Child(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentNullException(nameof(resourceName));
        }
        return Child(() => resourceName);
    }

    /// <summary>
    /// Flush all data of the offline database.
    /// </summary>
    /// <param name="localDatabase">
    /// Local database to flush. Leave <c>default</c> or <c>null</c> to flush default local database <see cref="FirebaseConfig.LocalDatabase"/>.
    /// </param>
    public void Flush(ILocalDatabase? localDatabase = default)
    {
        App.LocalDatabase.InternalDelete(localDatabase ?? App.Config.CachedLocalDatabase, new string[] { OfflineDatabaseIndicator });
    }

    private void Config_ImmediatePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.Config.DatabaseMaxConcurrentSyncWrites))
        {
            writeTaskPutControl.ConcurrentTokenCount = App.Config.DatabaseMaxConcurrentSyncWrites;
        }
    }

    #endregion

    #region DB Sync Helpers

    internal void DBCancelPut(string[] path)
    {
        if (writeTasks.TryRemove(path, out WriteTask writeTask))
        {
            writeTask.Dispose();
        }
    }

    internal bool DBIsWriting(string[] path)
    {
        return writeTasks.ContainsKey(path);
    }

    internal void DBPut(string? blob, string[] path, Action<(WriteTask writeTask, RetryExceptionEventArgs err)> onError, Action onSuccess)
    {
        WriteTask? writeTaskAdd = null;
        WriteTask writeTaskAdded = writeTasks.AddOrUpdate(path,
            k =>
            {
                writeTaskAdd = new WriteTask(App, path, blob,
                    () => writeTasks.TryRemove(path, out _),
                    onSuccess,
                    args =>
                    {
                        if (writeTaskAdd != null)
                        {
                            onError((writeTaskAdd, args));
                        }
                    });
                return writeTaskAdd;
            },
            (k, v) =>
            {
                if (v.Blob == blob)
                {
                    return v;
                }
                else
                {
                    writeTaskAdd = new WriteTask(App, path, blob,
                        () => writeTasks.TryRemove(path, out _),
                        onSuccess,
                        args =>
                        {
                            if (writeTaskAdd != null)
                            {
                                onError((writeTaskAdd, args));
                            }
                        });
                    return writeTaskAdd;
                }
            });
        if (writeTaskAdd == writeTaskAdded)
        {
            writeTaskAdd?.Run();
        }
    }

    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            App.Config.PropertyChanged -= Config_ImmediatePropertyChanged;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Helper Classes

    internal class WriteTask : Disposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string[] Path { get; }

        public string Uri { get; }

        public string? Blob { get; set; }

        public IFirebaseQuery Query { get; }

        private CancellationTokenSource tokenSource = new();

        private readonly Action finish;
        private readonly Action onSuccess;
        private readonly Action<RetryExceptionEventArgs> error;

        #endregion

        #region Initializers

        public WriteTask(
            RestfulFirebaseApp app,
            string[] path,
            string? blob,
            Action finish,
            Action onSuccess,
            Action<RetryExceptionEventArgs> error)
        {
            App = app;
            Path = path;
            Uri = "https://" + UrlUtilities.Combine(path.Skip(1));
            Blob = blob;
            this.finish = finish;
            this.onSuccess = onSuccess;
            this.error = error;
            Query = new ChildQuery(app, null, () => Uri);
        }

        #endregion

        #region Methods

        public async void Run()
        {
            if (tokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await App.Database.writeTaskPutControl.SendAsync(async delegate
                {
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        if (await Query.Put(() => Blob == null ? null : JsonConvert.SerializeObject(Blob), tokenSource.Token,
                            err =>
                            {
                                if (tokenSource.Token.IsCancellationRequested)
                                {
                                    return;
                                }

                                error?.Invoke(err);
                            }).ConfigureAwait(false))
                        {
                            onSuccess?.Invoke();
                        }
                    }
                    catch { }
                }, tokenSource.Token).ConfigureAwait(false);
            }
            catch { }

            if (tokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            finish?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                finish?.Invoke();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #endregion
}
