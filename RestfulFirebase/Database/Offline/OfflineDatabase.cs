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

        private class WriteTask
        {
            public RestfulFirebaseApp App { get; }

            public DataHolder Data { get; }

            public Action<RetryExceptionEventArgs> OnError { get; set; }

            public IFirebaseQuery Query { get; }

            public bool IsWritting { get; private set; }

            public bool HasPendingWrite { get; set; }

            public CancellationTokenSource CancellationSource { get; private set; }

            public WriteTask(RestfulFirebaseApp app, DataHolder data, Action<RetryExceptionEventArgs> onError)
            {
                App = app;
                Data = data;
                OnError = onError;
                Query = new ChildQuery(app, () => data.Uri);
                CancellationSource = new CancellationTokenSource();
            }

            public void Run()
            {
                HasPendingWrite = true;
                Task.Run(async delegate
                {
                    await Write();
                    lock (App.Database.OfflineDatabase.writeTasks)
                    {
                        App.Database.OfflineDatabase.writeTasks.Remove(this);
                    }
                });
            }

            public async Task Write()
            {
                if (CancellationSource.IsCancellationRequested) return;
                if (IsWritting) return;
                IsWritting = true;
                while (HasPendingWrite)
                {
                    if (CancellationSource.IsCancellationRequested) return;
                    HasPendingWrite = false;
                    await Query.Put(() =>
                    {
                        var blob = Data.Changes?.Blob;
                        return blob == null ? null : JsonConvert.SerializeObject(blob);
                    }, CancellationSource.Token, err =>
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
                                OnError(err);
                            }
                        }
                        else
                        {
                            OnError(err);
                        }
                    });
                }
                IsWritting = false;
            }
        }

        #endregion

        #region Properties

        internal const string Root = "offdb";
        internal static readonly string ShortPath = Utils.UrlCombine(Root, "short");
        internal static readonly string SyncBlobPath = Utils.UrlCombine(Root, "blob");
        internal static readonly string ChangesPath = Utils.UrlCombine(Root, "changes");

        public RestfulFirebaseApp App { get; }

        private List<WriteTask> writeTasks = new List<WriteTask>();

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
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
                string subUri = subPath.Substring(ShortPath.Length);
                if (Utils.UrlCompare(subUri, uri))
                {
                    if (includeOriginIfExists)
                    {
                        paths.Add(subUri);
                    }
                }
                else
                {
                    paths.Add(subUri);
                }
            }
            return paths;
        }

        public IEnumerable<string> GetHierUris(string uri, string baseUri = "", bool includeOriginIfExists = false)
        {
            if (string.IsNullOrEmpty(baseUri)) baseUri = App.Config.DatabaseURL;

            uri = uri.Trim().Trim('/');
            baseUri = baseUri.Trim().Trim('/');

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
                existing = writeTasks.FirstOrDefault(i => i.Data.Uri == data.Uri);
                if (existing != null)
                {
                    existing.HasPendingWrite = true;
                    existing.OnError = onError;
                }
                else
                {
                    var newWriteTask = new WriteTask(App, data, onError);
                    writeTasks.Add(newWriteTask);
                    newWriteTask.Run();
                }
            }
        }

        internal void CancelPut(DataHolder data)
        {
            WriteTask existing = null;
            lock (writeTasks)
            {
                existing = writeTasks.FirstOrDefault(i => Utils.UrlCompare(i.Data.Uri, data.Uri));
                if (existing != null)
                {
                    existing.CancellationSource.Cancel();
                }
            }
        }

        #endregion
    }
}
