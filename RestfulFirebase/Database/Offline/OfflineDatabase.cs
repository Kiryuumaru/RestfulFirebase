using Newtonsoft.Json;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            private string blob;
            public string Blob
            {
                get => blob;
                set
                {
                    HasPendingWrite = true;
                    blob = value;
                }
            }

            public Action<RetryExceptionEventArgs> OnError { get; set; }

            public IFirebaseQuery Query { get; }

            public bool IsWritting { get; private set; }

            public bool HasPendingWrite { get; private set; }

            public WriteTask(RestfulFirebaseApp app, string uri, string blob, Action<RetryExceptionEventArgs> onError)
            {
                App = app;
                Uri = uri;
                Blob = blob;
                OnError = onError;
                Query = new ChildQuery(app, () => uri);
            }

            public void Run()
            {
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
                if (IsWritting) return;
                IsWritting = true;
                while (HasPendingWrite)
                {
                    HasPendingWrite = false;
                    await Query.Put(() => Blob == null ? null : JsonConvert.SerializeObject(Blob), null, err =>
                    {
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

        public DataHolder GetData(string path)
        {
            var data = new DataHolder(App, path);
            return data.Exist ? data : null;
        }

        public IEnumerable<string> GetSubPaths(string path, bool includeBaseIfExists = false)
        {
            var paths = new List<string>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath, path)))
            {
                paths.Add(subPath.Substring(ShortPath.Length));
            }
            if (GetData(path) != null && includeBaseIfExists) paths.Add(path);
            return paths;
        }

        public IEnumerable<DataHolder> GetDatas(string path, bool includeBaseIfExists = false)
        {
            var datas = new List<DataHolder>();
            foreach (var subPath in GetSubPaths(path, includeBaseIfExists))
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

        internal void Put(string uri, string blob, Action<RetryExceptionEventArgs> onError)
        {
            uri = uri.EndsWith("/") ? uri : uri + "/";
            WriteTask existing = null;
            lock (writeTasks)
            {
                existing = writeTasks.FirstOrDefault(i => i.Uri == uri);
                if (existing != null)
                {
                    existing.Blob = blob;
                    existing.OnError = onError;
                }
                else
                {
                    var newWriteTask = new WriteTask(App, uri, blob, onError);
                    writeTasks.Add(newWriteTask);
                    newWriteTask.Run();
                }
            }
        }

        #endregion
    }
}
