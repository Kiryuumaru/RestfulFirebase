using Newtonsoft.Json;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    internal class OfflineDatabase : IDisposable
    {
        #region Helper Classes

        private class WriteTask
        {
            public RestfulFirebaseApp App { get; }

            public string Uri { get; }

            public string Blob { get; }

            public IFirebaseQuery Query { get; }

            public bool IsWritting { get; private set; }

            public bool IsCancelled => cancellationSource.IsCancellationRequested;

            public CancellationToken CancellationToken => cancellationSource.Token;

            private CancellationTokenSource cancellationSource;

            private Action<RetryExceptionEventArgs> error;

            public WriteTask(
                RestfulFirebaseApp app,
                string uri,
                string blob,
                Action<RetryExceptionEventArgs> error)
            {
                App = app;
                Uri = uri;
                Blob = blob;
                Query = new ChildQuery(app, () => uri);
                cancellationSource = new CancellationTokenSource();
                this.error = error;
            }

            public async Task Run()
            {
                if (IsWritting || IsCancelled)
                {
                    return;
                }
                IsWritting = true;

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
                                    error(err);
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

                IsWritting = false;
            }

            public void Cancel()
            {
                if (!IsCancelled)
                {
                    cancellationSource.Cancel();
                }
            }
        }

        #endregion

        #region Properties

        public RestfulFirebaseApp App { get; }

        public int WriteTaskCount => writeTasks.Count;

        internal const string Root = "offdb";
        internal static readonly string ShortPath = Utils.UrlCombine(Root, "short");
        internal static readonly string SyncBlobPath = Utils.UrlCombine(Root, "blob");
        internal static readonly string ChangesPath = Utils.UrlCombine(Root, "changes");

        private ConcurrentDictionary<DateTime, WriteTask> writeTasks = new ConcurrentDictionary<DateTime, WriteTask>();

        private bool islockControlRefresherRunning;
        private OperationInvoker writeTaskPutControl;
        private OperationInvoker writeTaskErrorControl;

        private ConcurrentDictionary<string, (long ticks, DataHolder holder)> DataHolderCache = new ConcurrentDictionary<string, (long ticks, DataHolder holder)>();

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
            writeTaskPutControl = new OperationInvoker(0);
            writeTaskErrorControl = new OperationInvoker(0);
        }

        #endregion

        #region Methods

        public async Task<DataHolder> GetData(string uri)
        {
            var data = BuildDataHolder(App, uri);
            return await data.IsExists().ConfigureAwait(false) ? data : null;
        }

        public async Task<string> GetBlob(string uri)
        {
            var data = BuildDataHolder(App, uri);
            return await data.GetBlob().ConfigureAwait(false);
        }

        public async Task<bool> IsExists(string uri)
        {
            var data = BuildDataHolder(App, uri);
            return await data.IsExists().ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetSubUris(string uri, bool includeOriginIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in await App.LocalDatabase.GetSubPaths(Utils.UrlCombine(ShortPath, uri)).ConfigureAwait(false))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (await IsExists(uri).ConfigureAwait(false) && includeOriginIfExists)
            {
                paths.Add(uri);
            }
            return paths;
        }

        public async Task<IEnumerable<string>> GetHierUris(string uri, string baseUri = "", bool includeOriginIfExists = false)
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
            var separated = Utils.UrlSeparate(path);
            var currentUri = baseUri;
            if (await IsExists(currentUri).ConfigureAwait(false))
            {
                hier.Add(currentUri);
            }    
            for (int i = 0; i < separated.Length - 1; i++)
            {
                currentUri = Utils.UrlCombine(currentUri, separated[i]);
                if (await IsExists(currentUri).ConfigureAwait(false))
                {
                    hier.Add(currentUri);
                }
            }
            if (await IsExists(currentUri).ConfigureAwait(false) && includeOriginIfExists)
            {
                hier.Add(uri);
            }
            return hier;
        }

        public async Task<IEnumerable<DataHolder>> GetDatas(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in await GetSubUris(uri, includeOriginIfExists).ConfigureAwait(false))
            {
                if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri)))
                {
                    datas.Add(BuildDataHolder(App, subUri));
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in await GetHierUris(uri, baseHierUri, includeOriginIfExists).ConfigureAwait(false))
                {
                    if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri)))
                    {
                        datas.Add(BuildDataHolder(App, subUri));
                    }
                }
            }
            return datas;
        }

        public async Task<IEnumerable<string>> GetDataUris(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<string>();
            foreach (var subUri in await GetSubUris(uri, includeOriginIfExists).ConfigureAwait(false))
            {
                if (!datas.Any(i => Utils.UrlCompare(i, subUri)))
                {
                    datas.Add(subUri);
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in await GetHierUris(uri, baseHierUri, includeOriginIfExists).ConfigureAwait(false))
                {
                    if (!datas.Any(i => Utils.UrlCompare(i, subUri)))
                    {
                        datas.Add(subUri);
                    }
                }
            }
            return datas;
        }

        public async Task<bool> HasChild(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in await GetSubUris(uri, includeOriginIfExists).ConfigureAwait(false))
            {
                if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri)))
                {
                    if (await GetBlob(subUri).ConfigureAwait(false) != null)
                    {
                        return true;
                    }
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in await GetHierUris(uri, baseHierUri, includeOriginIfExists).ConfigureAwait(false))
                {
                    if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri)))
                    {
                        if (await GetBlob(subUri).ConfigureAwait(false) != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<IEnumerable<DataHolder>> GetAllDatas()
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in await App.LocalDatabase.GetSubPaths(Utils.UrlCombine(ShortPath)).ConfigureAwait(false))
            {
                datas.Add(BuildDataHolder(App, subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public async Task Flush()
        {
            foreach (WriteTask task in writeTasks.Values)
            {
                task.Cancel();
            }
            var subPaths = await App.LocalDatabase.GetSubPaths(Root).ConfigureAwait(false);
            foreach (var subPath in subPaths)
            {
                await App.LocalDatabase.Delete(subPath).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            foreach (WriteTask task in writeTasks.Values)
            {
                task.Cancel();
            }
        }

        internal async Task Put(DataHolder data, Action<RetryExceptionEventArgs> onError)
        {
            var uris = (await GetDataUris(data.Uri, false, true).ConfigureAwait(false)).ToList();
            foreach (var uri in data.GetHierarchyUri())
            {
                if (await IsExists(uri).ConfigureAwait(false))
                {
                    uris.Add(uri);
                }
            }
            foreach (var uri in uris)
            {
                CancelPut(uri);
            }

            var existing = writeTasks.FirstOrDefault(i => Utils.UrlCompare(i.Value.Uri, data.Uri)).Value;
            if (existing != null)
            {
                if (existing.Blob != await data.GetBlob().ConfigureAwait(false))
                {
                    existing.Cancel();
                    QueueWrite(new WriteTask(App, data.Uri, (await data.GetChanges().ConfigureAwait(false))?.Blob, onError));
                }
            }
            else
            {
                QueueWrite(new WriteTask(App, data.Uri, (await data.GetChanges().ConfigureAwait(false))?.Blob, onError));
            }
        }

        internal void CancelPut(string uri)
        {
            foreach (var taskPair in writeTasks.Where(i => Utils.UrlCompare(i.Value.Uri, uri)))
            {
                taskPair.Value.Cancel();
            }
        }

        internal bool IsWriting(DataHolder data)
        {
            return writeTasks.FirstOrDefault(i => Utils.UrlCompare(i.Value.Uri, data.Uri)).Value?.IsCancelled ?? false;
        }

        internal void EvaluateCache(DataHolder dataHolder)
        {
            (long ticks, DataHolder holder) cache;
            if (DataHolderCache.TryGetValue(dataHolder.Uri, out cache))
            {
                cache.ticks = DateTime.UtcNow.Ticks;
            }
            else
            {
                cache = (DateTime.UtcNow.Ticks, dataHolder);
            }
            DataHolderCache.AddOrUpdate(dataHolder.Uri, cache, (oldKey, oldValue) => cache);

            while (DataHolderCache.Count != 0 && DataHolderCache.Count > App.Config.DatabaseInRuntimeDataCache)
            {
                try
                {
                    var lowestTick = DataHolderCache.Min(i => i.Value.ticks);
                    var keyOfLowest = DataHolderCache.FirstOrDefault(i => i.Value.ticks == lowestTick).Key;
                    DataHolderCache.TryRemove(keyOfLowest, out _);
                }
                catch { }
            }
        }

        internal DataHolder BuildDataHolder(RestfulFirebaseApp app, string uri)
        {
            uri = uri.EndsWith("/") ? uri : uri + "/";

            (long ticks, DataHolder holder) cache;
            if (DataHolderCache.TryGetValue(uri, out cache))
            {
                cache.ticks = DateTime.UtcNow.Ticks;
            }
            else
            {
                cache = (DateTime.UtcNow.Ticks, new DataHolder(app, uri));
            }
            DataHolderCache.AddOrUpdate(uri, cache, (oldKey, oldValue) => cache);

            while (DataHolderCache.Count != 0 && DataHolderCache.Count > App.Config.DatabaseInRuntimeDataCache)
            {
                try
                {
                    var lowestTick = DataHolderCache.Min(i => i.Value.ticks);
                    var keyOfLowest = DataHolderCache.FirstOrDefault(i => i.Value.ticks == lowestTick).Key;
                    DataHolderCache.TryRemove(keyOfLowest, out _);
                }
                catch { }
            }
            return cache.holder;
        }

        private void QueueWrite(WriteTask writeTask)
        {
            DateTime key = DateTime.UtcNow;

            writeTasks.AddOrUpdate(key, writeTask, (oldKey, oldValue) => writeTask);

            Task.Run(async delegate
            {
                await writeTask.Run().ContinueWith(delegate
                {
                    writeTasks.TryRemove(key, out _);
                }).ConfigureAwait(false);
            });

            StartLockControlRefresher();
        }

        private async void StartLockControlRefresher()
        {
            if (islockControlRefresherRunning)
            {
                return;
            }
            islockControlRefresherRunning = true;

            while (writeTasks.Count != 0)
            {
                App.Database.OfflineDatabase.writeTaskPutControl.ConcurrentTokenCount = App.Config.DatabaseMaxConcurrentWrites;
                await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
            }

            islockControlRefresherRunning = false;
        }

        #endregion
    }
}
