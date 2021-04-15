using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeHolder<T> : IDisposable
        where T : IRealtimeModel
    {
        public T Model { get; private set; }
        public FirebaseQuery Query { get; private set; }
        public IDisposable Subscription { get; private set; }

        internal RealtimeHolder(T realtime, FirebaseQuery query)
        {
            Model = realtime;
            Query = query;
        }

        public void Start()
        {
            if (Model.RealtimeWire != null) return;
            Model.BuildRealtimeWire(Query);
            Model.RealtimeWire.StartRealtime();
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(streamObject => { Model.RealtimeWire.ConsumeStream(streamObject); });
        }

        public void Delete()
        {
            Model.Delete();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
            Model.RealtimeWire?.StopRealtime();
        }
    }
}
