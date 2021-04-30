using Newtonsoft.Json;
using RestfulFirebase.Auth;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeWire : IDisposable
    {
        #region Properties

        private string jsonToPut;
        private CancellationTokenSource tokenSource;

        protected IDisposable Subscription;

        public RestfulFirebaseApp App { get; }
        public string Key { get; }
        public FirebaseQuery Query { get; }
        public bool InvokeSetFirst { get; private set; }
        public bool HasFirstStream { get; private set; }
        public bool IsWritting { get; private set; }
        public bool HasPendingWrite { get; private set; }

        public event Action OnStart;
        public event Action OnStop;
        public event EventHandler<WireChangesEventArgs> OnChanges;
        public event EventHandler<Exception> OnError;

        #endregion

        #region Initializers

        internal RealtimeWire(RestfulFirebaseApp app, FirebaseQuery parent, string key, bool invokeSetFirst)
        {
            App = app;
            Key = key;
            Query = new ChildQuery(parent.App, parent, () => key);
            InvokeSetFirst = invokeSetFirst;
        }

        #endregion

        #region Methods

        private async void Put(string data, EndNode node)
        {
            jsonToPut = JsonConvert.SerializeObject(data);
            HasPendingWrite = true;
            if (IsWritting) return;
            IsWritting = true;
            while (HasPendingWrite)
            {
                HasPendingWrite = false;
                await Query.Put(() => jsonToPut, null, err =>
                {
                    if (err.Exception.TaskCancelled)
                    {
                        err.Retry = true;
                    }
                    else if (err.Exception.InnerException is FirebaseAuthException)
                    {
                        err.Retry = true;
                    }
                    else
                    {
                        if (err.Exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            node.Changes = null;
                        }
                        OnError?.Invoke(this, err.Exception);
                    }
                });
            }
            IsWritting = false;
        }

        private void ConsumeEndNodeStream(StreamObject streamObject)
        {
            if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
            else if (streamObject.Path.Length == 1)
            {
                var blob = streamObject.Data;
                var path = Query.GetAbsolutePath();
                var offline = new EndNode(App, path);
                var latestBlob = offline.LatestBlob;

                if (IsWritting && HasPendingWrite) return;
                if (offline.Changes == null)
                {
                    if (blob == null) offline.Delete();
                    else offline.SyncBlob = blob;
                }
                else
                {
                    switch (offline.Changes.ChangesType)
                    {
                        case DataChangesType.Create:
                            if (blob == null)
                            {
                                Put(offline.Changes.Blob, offline);
                                return;
                            }
                            else
                            {
                                offline.SyncBlob = blob;
                                offline.Changes = null;
                                break;
                            }
                        case DataChangesType.Update:
                            if (blob == null)
                            {
                                offline.Delete();
                                break;
                            }
                            else if (offline.SyncBlob == blob)
                            {
                                Put(offline.Changes.Blob, offline);
                                return;
                            }
                            else
                            {
                                offline.SyncBlob = blob;
                                offline.Changes = null;
                                break;
                            }
                        case DataChangesType.Delete:
                            if (blob == null)
                            {
                                return;
                            }
                            else if (offline.SyncBlob == blob)
                            {
                                Put(null, offline);
                                return;
                            }
                            else
                            {
                                offline.SyncBlob = blob;
                                offline.Changes = null;
                                break;
                            }
                        case DataChangesType.None:
                            offline.SyncBlob = blob;
                            offline.Changes = null;
                            break;
                    }

                    if (latestBlob != offline.LatestBlob) OnChanges.Invoke(this, new WireChangesEventArgs(streamObject.Path));
                }
            }
        }

        public void InvokeStart() => OnStart?.Invoke();

        public void InvokeStop() => OnStop?.Invoke();

        public void InvokeStream(StreamObject streamObject)
        {
            if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");

            App.Database.OfflineDatabase.GetSubPaths();

            if (streamObject.Path.Length == 1) ConsumeEndNodeStream(streamObject);
            else
            {

            }

            HasFirstStream = true;
        }

        public void InvokeLocalChanges(StreamObject streamObject)
        {
            var blob = streamObject.Data;
            var path = Query.GetAbsolutePath();
            var offline = new EndNode(App, path);
            var latestBlob = offline.LatestBlob;

            if (offline.SyncBlob == null)
            {
                Put(blob, offline);
                offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.None : DataChangesType.Create);
            }
            else if (offline.Changes == null || offline.LatestBlob != blob)
            {
                Put(blob, offline);
                offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.Delete : DataChangesType.Update);
            }
        }

        public virtual void Start()
        {
            InvokeStart();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
            OnStop?.Invoke();
        }

        #endregion
    }
}
