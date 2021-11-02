using Newtonsoft.Json;
using ObservableHelpers;
using ObservableHelpers.Utilities;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    internal class OfflineDatabase : Disposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public int WriteTaskCount => writeTasks.Count;

        internal const string Root = "offdb";
        internal static readonly string ShortPath = UrlUtilities.Combine(Root, "short");
        internal static readonly string SyncBlobPath = UrlUtilities.Combine(Root, "blob");
        internal static readonly string ChangesPath = UrlUtilities.Combine(Root, "changes");

        private ConcurrentDictionary<string, WriteTask> writeTasks = new ConcurrentDictionary<string, WriteTask>();

        private OperationInvoker writeTaskPutControl;
        private OperationInvoker writeTaskErrorControl;

        private ConcurrentDictionary<DataHolderCacheKey, DataHolderCache> dataHolderCaches = new ConcurrentDictionary<DataHolderCacheKey, DataHolderCache>();
        private int dataHolderCachesCount = 0;

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
            writeTaskPutControl = new OperationInvoker(0);
            writeTaskErrorControl = new OperationInvoker(0);

            App.Config.ImmediatePropertyChanged += Config_PropertyChanged;

            UpdateWriteTaskLock();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                App.Config.ImmediatePropertyChanged -= Config_PropertyChanged;
                foreach (WriteTask task in writeTasks.Values)
                {
                    task.Cancel();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        public DataHolder GetData(ILocalDatabase localDatabase, string uri)
        {
            uri = uri.EndsWith("/") ? uri : uri + "/";

            DataHolderCacheKey key = new DataHolderCacheKey(uri, localDatabase);
            DataHolderCache cache = dataHolderCaches.GetOrAdd(key, _ =>
            {
                dataHolderCachesCount++;
                return new DataHolderCache(new DataHolder(App, uri, localDatabase));
            });
            key.Update(cache);

            while (dataHolderCachesCount != 0 && dataHolderCachesCount > App.Config.DatabaseInRuntimeDataCache)
            {
                try
                {
                    var lowestTick = dataHolderCaches.Min(i => i.Value.Ticks);
                    var keyOfLowest = dataHolderCaches.FirstOrDefault(i => i.Value.Ticks == lowestTick).Key;
                    if (dataHolderCaches.TryRemove(keyOfLowest, out _))
                    {
                        dataHolderCachesCount--;
                    }
                }
                catch { }
            }
            return cache.Holder;
        }

        public IEnumerable<string> GetSubUris(ILocalDatabase localDatabase, string uri, bool includeOriginIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(localDatabase, UrlUtilities.Combine(ShortPath, uri)))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (includeOriginIfExists)
            {
                if (GetData(localDatabase, uri).IsExists)
                {
                    paths.Add(uri);
                }
            }
            return paths;
        }

        public IEnumerable<string> GetHierUris(ILocalDatabase localDatabase, string uri, string baseUri = "", bool includeOriginIfExists = false)
        {
            if (string.IsNullOrEmpty(baseUri)) baseUri = App.Config.DatabaseURL;

            uri = uri.Trim();
            uri = uri.Trim('/');
            baseUri = baseUri.Trim();
            baseUri = baseUri.Trim('/');

            if (!uri.StartsWith(baseUri))
            {
                throw new Exception("URI not related");
            }

            var hier = new List<string>();
            var path = uri.Replace(baseUri, "");
            path = path.Trim('/');
            var separated = UrlUtilities.Separate(path);
            var currentUri = baseUri;
            if (GetData(localDatabase, currentUri).IsExists)
            {
                hier.Add(currentUri);
            }    
            for (int i = 0; i < separated.Length - 1; i++)
            {
                currentUri = UrlUtilities.Combine(currentUri, separated[i]);
                if (GetData(localDatabase, currentUri).IsExists)
                {
                    hier.Add(currentUri);
                }
            }
            if (GetData(localDatabase, currentUri).IsExists && includeOriginIfExists)
            {
                hier.Add(uri);
            }
            return hier;
        }

        public IEnumerable<DataHolder> GetDatas(ILocalDatabase localDatabase, string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in GetSubUris(localDatabase, uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                {
                    datas.Add(GetData(localDatabase, subUri));
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(localDatabase, uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                    {
                        datas.Add(GetData(localDatabase, subUri));
                    }
                }
            }
            return datas;
        }

        public IEnumerable<string> GetDataUris(ILocalDatabase localDatabase, string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<string>();
            foreach (var subUri in GetSubUris(localDatabase, uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i, subUri)))
                {
                    datas.Add(subUri);
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(localDatabase, uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i, subUri)))
                    {
                        datas.Add(subUri);
                    }
                }
            }
            return datas;
        }

        public bool HasChild(ILocalDatabase localDatabase, string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in GetSubUris(localDatabase, uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                {
                    if (GetData(localDatabase, subUri).Blob != null)
                    {
                        return true;
                    }
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(localDatabase, uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                    {
                        if (GetData(localDatabase, subUri).Blob != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public IEnumerable<DataHolder> GetAllDatas(ILocalDatabase localDatabase)
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(localDatabase, ShortPath))
            {
                datas.Add(GetData(localDatabase, subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public void Flush(ILocalDatabase localDatabase)
        {
            foreach (WriteTask task in writeTasks.Values)
            {
                task.Cancel();
            }
            var subPaths = App.LocalDatabase.GetSubPaths(localDatabase, Root);
            foreach (var subPath in subPaths)
            {
                App.LocalDatabase.Delete(localDatabase, subPath);
            }
        }

        internal void Put(DataHolder data, Action onWrite, Action<RetryExceptionEventArgs> onError)
        {
            if (writeTasks.TryGetValue(data.Uri, out WriteTask writeTask))
            {
                writeTask.Error += onError;
                writeTask.Finish += onWrite;
                if (writeTask.Blob != data.Blob)
                {
                    writeTask.ReWriteRequested = true;
                    writeTask.Blob = data.Blob;
                }
            }
            else
            {
                writeTask = new WriteTask(App, data.Uri, data.Changes?.Blob);
                writeTask.Error += onError;
                writeTask.Finish += delegate
                {
                    writeTasks.TryRemove(data.Uri, out _);
                    onWrite();
                };
                writeTasks.TryAdd(data.Uri, writeTask);
                writeTask.Run();
            }
        }

        internal void CancelPut(string uri)
        {
            foreach (var taskPair in writeTasks.Where(i => UrlUtilities.Compare(i.Value.Uri, uri)))
            {
                taskPair.Value.Cancel();
            }
        }

        internal bool IsWriting(DataHolder data)
        {
            return writeTasks.FirstOrDefault(i => UrlUtilities.Compare(i.Value.Uri, data.Uri)).Value?.IsCancelled ?? false;
        }

        internal void EvaluateCache(DataHolder dataHolder)
        {
            bool isNew = false;
            DataHolderCacheKey key = new DataHolderCacheKey(dataHolder.Uri, dataHolder.LocalDatabase);
            DataHolderCache cache = dataHolderCaches.GetOrAdd(key, _ =>
            {
                isNew = true;
                dataHolderCachesCount++;
                return new DataHolderCache(dataHolder);
            });
            if (!isNew)
            {
                cache.Ticks = DateTime.UtcNow.Ticks;
            }
            key.Update(cache);

            while (dataHolderCachesCount != 0 && dataHolderCachesCount > App.Config.DatabaseInRuntimeDataCache)
            {
                try
                {
                    var lowestTick = dataHolderCaches.Min(i => i.Value.Ticks);
                    var keyOfLowest = dataHolderCaches.FirstOrDefault(i => i.Value.Ticks == lowestTick).Key;
                    if (dataHolderCaches.TryRemove(keyOfLowest, out _))
                    {
                        dataHolderCachesCount--;
                    }
                }
                catch { }
            }
        }

        private void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.Config.DatabaseMaxConcurrentWrites))
            {
                UpdateWriteTaskLock();
            }
        }

        private void UpdateWriteTaskLock()
        {
            writeTaskPutControl.ConcurrentTokenCount = App.Config.DatabaseMaxConcurrentWrites;
        }

        #endregion

        #region Helper Classes

        private class WriteTask
        {
            public RestfulFirebaseApp App { get; }

            public string Uri { get; }

            public string Blob { get; set; }

            public IFirebaseQuery Query { get; }

            public bool ReWriteRequested { get; set; }

            public bool IsWritting { get; private set; }

            public bool IsCancelled => cancellationSource.IsCancellationRequested;

            public CancellationToken CancellationToken => cancellationSource.Token;

            public event Action<RetryExceptionEventArgs> Error;
            public event Action Finish;

            private CancellationTokenSource cancellationSource;

            public WriteTask(
                RestfulFirebaseApp app,
                string uri,
                string blob)
            {
                App = app;
                Uri = uri;
                Blob = blob;
                Query = new ChildQuery(app, () => uri);
                cancellationSource = new CancellationTokenSource();
            }

            public async void Run()
            {
                if (IsWritting || IsCancelled)
                {
                    Finish?.Invoke();
                    return;
                }
                IsWritting = true;

                do
                {
                    try
                    {
                        await App.Database.OfflineDatabase.writeTaskPutControl.SendAsync(async delegate
                        {
                            if (IsCancelled)
                            {
                                return;
                            }
                            try
                            {
                                App.Database.OfflineDatabase.writeTaskErrorControl.ConcurrentTokenCount = App.Config.DatabaseMaxConcurrentWrites;
                                await Query.Put(() => Blob == null ? null : JsonConvert.SerializeObject(Blob), cancellationSource.Token, err =>
                                {
                                    if (IsCancelled)
                                    {
                                        return;
                                    }
                                    App.Database.OfflineDatabase.writeTaskErrorControl.ConcurrentTokenCount = 1;
                                    Type exType = err.Exception.GetType();
                                    if (err.Exception is OfflineModeException)
                                    {
                                        err.Retry = App.Database.OfflineDatabase.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }
                                    else if (err.Exception is OperationCanceledException)
                                    {
                                        err.Retry = App.Database.OfflineDatabase.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }
                                    else if (err.Exception is AuthException)
                                    {
                                        err.Retry = App.Database.OfflineDatabase.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }
                                    else
                                    {
                                        Error?.Invoke(err);
                                    }
                                }).ConfigureAwait(false);
                            }
                            catch { }
                            if (!IsCancelled)
                            {
                                App.Database.OfflineDatabase.writeTaskErrorControl.ConcurrentTokenCount = App.Config.DatabaseMaxConcurrentWrites;
                            }
                        }, cancellationSource.Token).ConfigureAwait(false);
                    }
                    catch { }
                }
                while (ReWriteRequested);

                IsWritting = false;

                Finish?.Invoke();
            }

            public void Cancel()
            {
                if (!IsCancelled)
                {
                    cancellationSource.Cancel();
                }
            }
        }

        private class DataHolderCache
        {
            public DataHolder Holder { get; }
            public long Ticks { get; set; }

            public DataHolderCache(DataHolder holder)
            {
                Holder = holder;
                Ticks = DateTime.UtcNow.Ticks;
            }
        }

        private struct DataHolderCacheKey
        {
            public string Url { get => dataHolderCache?.Holder?.Uri ?? url; }
            public ILocalDatabase LocalDatabase { get => dataHolderCache?.Holder?.LocalDatabase ?? localDatabase; }

            private string url;
            private ILocalDatabase localDatabase;
            private DataHolderCache dataHolderCache;

            public DataHolderCacheKey(string url, ILocalDatabase localDatabase)
            {
                this.url = url;
                this.localDatabase = localDatabase;
                dataHolderCache = null;
            }

            public DataHolderCacheKey(DataHolderCache dataHolderCache)
            {
                this.url = null;
                this.localDatabase = null;
                this.dataHolderCache = dataHolderCache;
            }

            public void Update(DataHolderCache dataHolderCache)
            {
                this.dataHolderCache = dataHolderCache;
            }

            public override bool Equals(object obj)
            {
                return obj is DataHolderCacheKey key &&
                       UrlUtilities.Compare(Url, key.Url) &&
                       EqualityComparer<ILocalDatabase>.Default.Equals(LocalDatabase, key.LocalDatabase);
            }

            public override int GetHashCode()
            {
                return -1887812163 + EqualityComparer<ILocalDatabase>.Default.GetHashCode(LocalDatabase);
            }
        }

        #endregion
    }
}
