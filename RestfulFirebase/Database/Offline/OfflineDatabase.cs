using Newtonsoft.Json;
using RestfulFirebase.Database.Query;
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

        private const int MaxWriteTokenCount = 1000;
        private int lastMaxConcurrentWrites = 0;

        private class WriteTask
        {
            public RestfulFirebaseApp App { get; }

            public string Uri { get; }

            public string Blob { get; }

            public IFirebaseQuery Query { get; }

            public bool IsWritting { get; private set; }

            public CancellationTokenSource CancellationSource { get; private set; }

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
                CancellationSource = new CancellationTokenSource();
                this.error = error;
            }

            public void Run()
            {
                if (IsWritting) return;
                IsWritting = true;

                Task.Run(async delegate
                {
                    await App.Database.OfflineDatabase.semaphoreSlimControl.WaitAsync();
                    try
                    {
                        if (App.Config.DatabaseMaxConcurrentWrites > MaxWriteTokenCount) throw new Exception("DatabaseMaxConcurrentWrites exceeds MaxWriteTokenCount");
                        if (App.Database.OfflineDatabase.lastMaxConcurrentWrites < App.Config.DatabaseMaxConcurrentWrites)
                        {
                            int tokenToRelease = App.Config.DatabaseMaxConcurrentWrites - App.Database.OfflineDatabase.lastMaxConcurrentWrites;
                            App.Database.OfflineDatabase.lastMaxConcurrentWrites = App.Config.DatabaseMaxConcurrentWrites;
                            App.Database.OfflineDatabase.semaphoreSlim.Release(tokenToRelease);
                        }
                        else if (App.Database.OfflineDatabase.lastMaxConcurrentWrites > App.Config.DatabaseMaxConcurrentWrites)
                        {
                            int tokenToWait = App.Database.OfflineDatabase.lastMaxConcurrentWrites - App.Config.DatabaseMaxConcurrentWrites;
                            App.Database.OfflineDatabase.lastMaxConcurrentWrites = App.Config.DatabaseMaxConcurrentWrites;
                            for (int i = 0; i < tokenToWait; i++)
                            {
                                await App.Database.OfflineDatabase.semaphoreSlim.WaitAsync();
                            }
                        }
                    }
                    catch { }
                    App.Database.OfflineDatabase.semaphoreSlimControl.Release();
                });

                Task.Run(async delegate
                {
                    await Write().ConfigureAwait(false);
                    lock (App.Database.OfflineDatabase.writeTasks)
                    {
                        App.Database.OfflineDatabase.writeTasks.Remove(this);
                    }
                }).ConfigureAwait(false);
            }

            private async Task Write()
            {
                if (CancellationSource.IsCancellationRequested) return;
                await App.Database.OfflineDatabase.semaphoreSlim.WaitAsync().ConfigureAwait(false);
                if (!CancellationSource.IsCancellationRequested)
                {
                    await Query.Put(() => Blob == null ? null : JsonConvert.SerializeObject(Blob), CancellationSource.Token, err =>
                    {
                        if (CancellationSource.IsCancellationRequested) return;
                        Type exType = err.Exception.GetType();
                        if (err.Exception is FirebaseException firEx)
                        {
                            if (firEx.Reason == FirebaseExceptionReason.OfflineMode)
                            {
                                err.Retry = true;
                            }
                            else if (firEx.Reason == FirebaseExceptionReason.OperationCancelled)
                            {
                                err.Retry = true;
                            }
                            else if (firEx.Reason == FirebaseExceptionReason.Auth)
                            {
                                err.Retry = true;
                            }
                            else
                            {
                                error(err);
                            }
                        }
                        else
                        {
                            error(err);
                        }
                    }).ConfigureAwait(false);
                }
                App.Database.OfflineDatabase.semaphoreSlim.Release();
                IsWritting = false;
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

        private List<WriteTask> writeTasks = new List<WriteTask>();

        private SemaphoreSlim semaphoreSlim;
        private SemaphoreSlim semaphoreSlimControl;

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
            semaphoreSlim = new SemaphoreSlim(0, MaxWriteTokenCount);
            semaphoreSlimControl = new SemaphoreSlim(1, 1);
        }

        #endregion

        #region Methods

        public DataHolder GetData(string uri)
        {
            var data = new DataHolder(App, uri);
            return data.Exist ? data : null;
        }

        public IEnumerable<string> GetSubUris(string uri, bool includeOriginIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.UrlCombine(ShortPath, uri)))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (GetData(uri) != null && includeOriginIfExists) paths.Add(uri);
            return paths;
        }

        public IEnumerable<string> GetHierUris(string uri, string baseUri = "", bool includeOriginIfExists = false)
        {
            if (string.IsNullOrEmpty(baseUri)) baseUri = App.Config.DatabaseURL;

            uri = uri.Trim();
            uri = uri.Trim('/');
            baseUri = baseUri.Trim();
            baseUri = baseUri.Trim('/');

            if (!uri.StartsWith(baseUri)) throw new Exception("URI not related");

            var hier = new List<string>();
            var path = uri.Replace(baseUri, "");
            path = path.Trim('/');
            var separated = Utils.UrlSeparate(path);
            var currentUri = baseUri;
            if (GetData(currentUri) != null) hier.Add(currentUri);
            for (int i = 0; i < separated.Length - 1; i++)
            {
                currentUri = Utils.UrlCombine(currentUri, separated[i]);
                if (GetData(currentUri) != null) hier.Add(currentUri);
            }
            if (GetData(uri) != null && includeOriginIfExists) hier.Add(uri);
            return hier;
        }

        public IEnumerable<DataHolder> GetDatas(string uri, bool includeOriginIfExists = false, bool includeHierIfExists = false, string baseHierUri = "")
        {
            var datas = new List<DataHolder>();
            foreach (var subUri in GetSubUris(uri, includeOriginIfExists))
            {
                if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri))) datas.Add(new DataHolder(App, subUri));
            }
            if (includeHierIfExists)
            {
                foreach (var subUri in GetHierUris(uri, baseHierUri, includeOriginIfExists))
                {
                    if (!datas.Any(i => Utils.UrlCompare(i.Uri, subUri))) datas.Add(new DataHolder(App, subUri));
                }
            }
            return datas;
        }

        public IEnumerable<DataHolder> GetAllDatas()
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.UrlCombine(ShortPath)))
            {
                datas.Add(new DataHolder(App, subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public void Flush()
        {
            var subPaths = App.LocalDatabase.GetSubPaths(Root);
            foreach (var subPath in subPaths)
            {
                App.LocalDatabase.Delete(subPath);
            }
        }

        public void Dispose()
        {
            lock (writeTasks)
            {
                foreach (var writeTask in writeTasks)
                {
                    writeTask.CancellationSource.Cancel();
                }
            }
        }

        internal void Put(DataHolder data, Action<RetryExceptionEventArgs> onError)
        {
            var datas = GetDatas(data.Uri, false, true).ToList();
            foreach (var uri in data.HierarchyUri)
            {
                var hierData = GetData(uri);
                if (hierData != null) datas.Add(hierData);
            }
            foreach (var subData in datas)
            {
                CancelPut(subData);
            }

            WriteTask existing = null;
            lock (writeTasks)
            {
                existing = writeTasks.FirstOrDefault(i => Utils.UrlCompare(i.Uri, data.Uri));
                if (existing != null)
                {
                    existing.CancellationSource.Cancel();
                }
                var newWriteTask = new WriteTask(App, data.Uri, data.Changes?.Blob, onError);
                writeTasks.Add(newWriteTask);
                newWriteTask.Run();
            }
        }

        internal void CancelPut(DataHolder data)
        {
            lock (writeTasks)
            {
                foreach (WriteTask task in writeTasks.Where(i => Utils.UrlCompare(i.Uri, data.Uri)))
                {
                    task.CancellationSource.Cancel();
                }
            }
        }

        #endregion
    }
}
