using RestfulFirebase.Auth;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeWire : IDisposable
    {
        private string jsonToPut;
        private CancellationTokenSource tokenSource;

        protected IDisposable Subscription;

        public void InvokeStart() => OnStart?.Invoke();
        public void InvokeStop() => OnStop?.Invoke();
        public bool InvokeStream(StreamObject streamObject)
        {
            var hasChanges = OnStream?.Invoke(streamObject) ?? false;
            HasFirstStream = true;
            return hasChanges;
        }

        public string Key { get; private set; }
        public FirebaseQuery Query { get; private set; }
        public bool InvokeSetFirst { get; private set; }
        public bool HasFirstStream { get; private set; }
        public bool IsWritting { get; private set; }
        public bool HasPendingWrite { get; private set; }

        public event Action OnStart;
        public event Action OnStop;
        public event Func<StreamObject, bool> OnStream;

        internal RealtimeWire(string key, FirebaseQuery parent, bool invokeSetFirst)
        {
            Key = key;
            Query = new ChildQuery(parent.App, parent, () => key);
            InvokeSetFirst = invokeSetFirst;
        }

        public RealtimeWire Child(string key)
        {
            return new RealtimeWire(key, Query, InvokeSetFirst);
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
    }

    public class RealtimeWire<T> : RealtimeWire
        where T : IRealtimeModel
    {
        public T Model { get; private set; }

        internal RealtimeWire(T model, FirebaseQuery parent, bool invokeSetFirst)
            : base (model.Key, parent, invokeSetFirst)
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
