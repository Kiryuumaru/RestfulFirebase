using RestfulFirebase.Auth;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeWire : IDisposable
    {
        #region Properties

        private string jsonToPut;
        //private bool isStreamWaiting;
        private StreamObject streamObjectBuffer;

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

        internal RealtimeWire(RestfulFirebaseApp app, string key, FirebaseQuery parent, bool invokeSetFirst)
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
            streamObjectBuffer = streamObject;
            // FIX LATER
            //if (IsWritting && HasPendingWrite) return false;
            //{
            //    if (isStreamWaiting) return false;
            //    isStreamWaiting = true;
            //    while (IsWritting && HasPendingWrite) { }
            //    isStreamWaiting = false;
            //}
            var hasChanges = OnStream?.Invoke(streamObjectBuffer) ?? false;
            HasFirstStream = true;
            return hasChanges;
        }

        public RealtimeWire Child(string key, bool invokeSetFirst)
        {
            return new RealtimeWire(App, key, Query, invokeSetFirst);
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

    public class RealtimeWire<T> : RealtimeWire
        where T : IRealtimeModel
    {
        public T Model { get; private set; }

        internal RealtimeWire(RestfulFirebaseApp app, string key, T model, FirebaseQuery parent, bool invokeSetFirst)
            : base (app, key, parent, invokeSetFirst)
        {
            Model = model;
        }

        public override void Start()
        {
            Model.MakeRealtime(this);
            InvokeStart();
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(streamObject => { InvokeStream(streamObject); });
        }
    }
}
