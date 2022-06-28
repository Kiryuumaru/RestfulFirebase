using RestfulFirebase.RealtimeDatabase.Query;
using RestfulFirebase.Utilities;
using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using RestfulFirebase.Exceptions;
using System.Threading.Tasks;
using RestfulFirebase.RealtimeDatabase.Realtime;
using DisposableHelpers;
using LockerHelpers;
using System.Text.Json;

namespace RestfulFirebase.RealtimeDatabase;

/// <summary>
/// App module that provides firebase realtime database implementations.
/// </summary>
public class RealtimeDatabaseApp : Disposable
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; private set; }

    internal const string OfflineDatabaseIndicator = "rtdb";

    private readonly OperationInvoker writeTaskPutControl = new(0);
    private readonly ConcurrentDictionary<string[], WriteTask> writeTasks = new(PathEqualityComparer.Instance);

    #endregion

    #region Initializers

    internal RealtimeDatabaseApp(RestfulFirebaseApp app)
    {
        App = app;

        App.Config.PropertyChanged += Config_PropertyChanged;

        writeTaskPutControl.ConcurrentTokenCount = App.Config.RealtimeDatabaseMaxConcurrentSyncWrites;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates new instance of <see cref="RealtimeDatabase"/> database with the specified <paramref name="databaseUrl"/>.
    /// </summary>
    /// <param name="databaseUrl">
    /// The URL of the database (i.e., "https://projectid-default-rtdb.firebaseio.com/").
    /// </param>
    /// <returns>
    /// The created <see cref="RealtimeDatabase"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="databaseUrl"/> is null or empty.
    /// </exception>
    public RealtimeDatabase Database(string databaseUrl)
    {
        if (databaseUrl == null)
        {
            throw new ArgumentNullException(nameof(databaseUrl));
        }

        return new RealtimeDatabase(App, databaseUrl);
    }

    /// <summary>
    /// Flush all data of the offline database.
    /// </summary>
    /// <param name="localDatabase">
    /// Local database to flush. Leave <c>default</c> or <c>null</c> to flush default local database <see cref="FirebaseConfig.LocalDatabase"/>.
    /// </param>
    public void Flush(ILocalDatabase? localDatabase = default)
    {
        App.LocalDatabase.Delete(localDatabase ?? App.Config.LocalDatabase, new string[] { OfflineDatabaseIndicator });
    }

    private void Config_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.Config.RealtimeDatabaseMaxConcurrentSyncWrites))
        {
            writeTaskPutControl.ConcurrentTokenCount = App.Config.RealtimeDatabaseMaxConcurrentSyncWrites;
        }
    }

    #endregion

    #region DB Sync Helpers

    internal void DBCancelPut(string[] path)
    {
        if (writeTasks.TryRemove(path, out WriteTask? writeTask))
        {
            writeTask?.Dispose();
        }
    }

    internal bool DBIsWriting(string[] path)
    {
        return writeTasks.ContainsKey(path);
    }

    internal void DBPut(RealtimeDatabase realtimeDatabase, string[] path, string? blob, Action<(WriteTask writeTask, RetryExceptionEventArgs err)> onError, Action onSuccess)
    {
        WriteTask? writeTaskAdd = null;
        WriteTask writeTaskAdded = writeTasks.AddOrUpdate(path,
            k =>
            {
                writeTaskAdd = new WriteTask(realtimeDatabase, path, blob,
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
                    writeTaskAdd = new WriteTask(realtimeDatabase, path, blob,
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
            App.Config.PropertyChanged -= Config_PropertyChanged;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Helper Classes

    internal class WriteTask : Disposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string? Blob { get; set; }

        public string[] Path { get; }

        public string Uri { get; }

        public IFirebaseQuery Query { get; }

        private readonly CancellationTokenSource tokenSource = new();

        private readonly Action finish;
        private readonly Action onSuccess;
        private readonly Action<RetryExceptionEventArgs> error;

        #endregion

        #region Initializers

        public WriteTask(
            RealtimeDatabase realtimeDatabase,
            string[] path,
            string? blob,
            Action finish,
            Action onSuccess,
            Action<RetryExceptionEventArgs> error)
        {
            App = realtimeDatabase.App;
            Path = path;
            Blob = blob;
            Query = new ChildQuery(realtimeDatabase, null, UrlUtilities.Combine(path.Skip(2)));
            Uri = Query.GetAbsoluteUrl();

            this.finish = finish;
            this.onSuccess = onSuccess;
            this.error = error;

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
                await App.RealtimeDatabase.writeTaskPutControl.SendAsync(async delegate
                {
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        if (await Query.Put(() => Blob == null ? null : JsonSerializer.Serialize(Blob), tokenSource.Token,
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
