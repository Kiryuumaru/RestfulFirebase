using RestfulFirebase.Auth;
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
        public event Func<StreamObject, bool> OnStream;

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

        public void InvokeStart() => OnStart?.Invoke();

        public void InvokeStop() => OnStop?.Invoke();

        public bool InvokeStream(StreamObject streamObject)
        {
            bool hasChanges = false;

            if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
            else if (streamObject.Path.Length == 1)
            {

            }
            else
            {

            }

            HasFirstStream = true;
            return hasChanges;
        }

        public async void Put(string json, Action<RetryExceptionEventArgs<FirebaseDatabaseException>> onError)
        {
            jsonToPut = json;
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
                        onError(err);
                    }
                });
            }
            IsWritting = false;
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
