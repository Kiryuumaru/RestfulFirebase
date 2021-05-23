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
        internal static readonly string ShortPath = Utils.CombineUrl(Root, "short");
        internal static readonly string SyncBlobPath = Utils.CombineUrl(Root, "blob");
        internal static readonly string ChangesPath = Utils.CombineUrl(Root, "changes");

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

        public IEnumerable<string> GetSubPaths(string uri, bool includeBaseIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath, uri)))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (GetData(uri) != null && includeBaseIfExists) paths.Add(uri);
            return paths;
        }

        public IEnumerable<DataHolder> GetDatas(string uri, bool includeBaseIfExists = false)
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in GetSubPaths(uri, includeBaseIfExists))
            {
                datas.Add(new DataHolder(App, subPath));
            }
            return datas;
        }

        public IEnumerable<DataHolder> GetAllDatas()
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath)))
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

        }

        internal void Put(DataHolder data, Action<RetryExceptionEventArgs> onError)
        {
            var datas = GetDatas(data.Uri).ToList();
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
                existing = writeTasks.FirstOrDefault(i => i.Data.Uri == data.Uri);
                if (existing != null)
                {
                    existing.CancellationSource.Cancel();
                }
            }
        }

        #endregion
    }
}
