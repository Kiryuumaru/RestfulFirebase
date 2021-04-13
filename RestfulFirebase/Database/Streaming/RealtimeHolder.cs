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
        public bool InvokeSetFirst { get; private set; }
        public IDisposable Subscription { get; private set; }

        internal RealtimeHolder(T realtime, FirebaseQuery query, bool invokeSetFirst)
        {
            Model = realtime;
            Query = query;
            InvokeSetFirst = invokeSetFirst;
        }

        public void Start()
        {
            Model.StartRealtime(Query, InvokeSetFirst);
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(Model.RealtimeWire.ConsumeStream);
        }

        public void Delete()
        {
            Model.Delete();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
        }
    }
}
