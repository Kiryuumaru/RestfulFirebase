using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public abstract class RealtimeWire : IDisposable
    {
        protected IDisposable Subscription;

        public FirebaseQuery Query { get; protected set; }

        public event Action OnStart;
        public event Action OnStop;
        public event Func<StreamObject, bool> OnStream;

        protected void InvokeStart() => OnStart?.Invoke();
        protected void InvokeStop() => OnStop?.Invoke();
        protected bool InvokeStream(StreamObject streamObject) => OnStream?.Invoke(streamObject) ?? false;

        public abstract void Start();

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

        internal RealtimeWire(T model, FirebaseQuery parent)
        {
            Model = model;
            Query = new ChildQuery(parent.App, parent, () => model.Key);
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
