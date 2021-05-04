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

        private async void Put(string data, DataNode node)
        {
            //jsonToPut = JsonConvert.SerializeObject(data);
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

        private void ConsumeNodeStream(StreamObject streamObject)
        {

        }

        public void InvokeStart() => OnStart?.Invoke();

        public void InvokeStop() => OnStop?.Invoke();

        public void InvokeStream(StreamObject streamObject)
        {
            if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");

            //App.Database.OfflineDatabase.GetSubPaths();

            if (streamObject.Path.Length == 1) ConsumeNodeStream(streamObject);
            else
            {

            }

            HasFirstStream = true;
        }

        public void InvokeLocalChanges(StreamObject streamObject)
        {

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
