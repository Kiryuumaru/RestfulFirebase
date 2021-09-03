using Newtonsoft.Json;
using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    internal class OfflineDatabase : Disposable
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

            public async void Run(Action onFinish)
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

                onFinish?.Invoke();
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
        internal static readonly string ShortPath = UrlUtilities.Combine(Root, "short");
        internal static readonly string SyncBlobPath = UrlUtilities.Combine(Root, "blob");
        internal static readonly string ChangesPath = UrlUtilities.Combine(Root, "changes");

        private ConcurrentDictionary<string, WriteTask> writeTasks = new ConcurrentDictionary<string, WriteTask>();

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

        public DataHolder GetData(string uri)
        {
            uri = uri.EndsWith("/") ? uri : uri + "/";

            (long ticks, DataHolder holder) cache;
            if (DataHolderCache.TryGetValue(uri, out cache))
            {
                cache.ticks = DateTime.UtcNow.Ticks;
            }
            else
            {
                cache = (DateTime.UtcNow.Ticks, new DataHolder(App, uri));
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

        public IEnumerable<string> GetSubUris(string uri, bool includeOriginIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(UrlUtilities.Combine(ShortPath, uri)))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (includeOriginIfExists)
            {
                if (GetData(uri).IsExists)
                {
                    paths.Add(uri);
                }
            }
            return paths;
        }

        public IEnumerable<string> GetHierUris(string uri, string baseUri = "", bool includeOriginIfExists = false)
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
            if (GetData(currentUri).IsExists)
            {
                hier.Add(currentUri);
            }    
            for (int i = 0; i < separated.Length - 1; i++)
            {
                currentUri = UrlUtilities.Combine(currentUri, separated[i]);
                if (GetData(currentUri).IsExists)
                {
                    hier.Add(currentUri);
                }
            }
            if (GetData(currentUri).IsExists && includeOriginIfExists)
            {
                hier.Add(uri);
            }
            return hier;
        }

        public IEnumerable<DataHolder> GetDatas(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in GetSubUris(uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                {
                    datas.Add(GetData(subUri));
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                    {
                        datas.Add(GetData(subUri));
                    }
                }
            }
            return datas;
        }

        public IEnumerable<string> GetDataUris(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<string>();
            foreach (var subUri in GetSubUris(uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i, subUri)))
                {
                    datas.Add(subUri);
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i, subUri)))
                    {
                        datas.Add(subUri);
                    }
                }
            }
            return datas;
        }

        public bool HasChild(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in GetSubUris(uri, includeOriginIfExists))
            {
                if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                {
                    if (GetData(subUri).Blob != null)
                    {
                        return true;
                    }
                }
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => UrlUtilities.Compare(i.Uri, subUri)))
                    {
                        if (GetData(subUri).Blob != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public IEnumerable<DataHolder> GetAllDatas()
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(ShortPath))
            {
                datas.Add(GetData(subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public void Flush()
        {
            foreach (WriteTask task in writeTasks.Values)
            {
                task.Cancel();
            }
            var subPaths = App.LocalDatabase.GetSubPaths(Root);
            foreach (var subPath in subPaths)
            {
                App.LocalDatabase.Delete(subPath);
            }
        }

        internal void Put(DataHolder data, Action onWrite, Action<RetryExceptionEventArgs> onError)
        {
            var uris = GetDataUris(data.Uri, false, true).ToList();
            foreach (var uri in data.HierarchyUri)
            {
                if (GetData(uri).IsExists)
                {
                    uris.Add(uri);
                }
            }
            foreach (var uri in uris)
            {
                CancelPut(uri);
            }

            var existing = writeTasks.FirstOrDefault(i => UrlUtilities.Compare(i.Value.Uri, data.Uri)).Value;
            if (existing != null)
            {
                if (existing.Blob != data.Blob)
                {
                    existing.Cancel();
                    QueueWrite(new WriteTask(App, data.Uri, data.Changes?.Blob, onError), onWrite);
                }
            }
            else
            {
                QueueWrite(new WriteTask(App, data.Uri, data.Changes?.Blob, onError), onWrite);
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

        private void QueueWrite(WriteTask writeTask, Action onWrite)
        {
            string key;

            do
            {
                key = UIDFactory.GenerateUID();
            }
            while (!writeTasks.TryAdd(key, writeTask));

            writeTask.Run(delegate
            {
                writeTasks.TryRemove(key, out _);
                onWrite?.Invoke();
            });
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
    }
}
