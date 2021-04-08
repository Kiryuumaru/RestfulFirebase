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
        public T RealtimeModel { get; private set; }
        public FirebaseQuery Query { get; private set; }
        public bool InvokeSetFirst { get; private set; }
        public IDisposable Subscription { get; private set; }

        internal RealtimeHolder(T realtime, FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeModel = realtime;
            Query = query;
            InvokeSetFirst = invokeSetFirst;
        }

        public void Start()
        {
            RealtimeModel.StartRealtime(Query, InvokeSetFirst, out Action<StreamObject> onNext);
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => RealtimeModel.OnError(e)).Run())
                .Subscribe(onNext);
        }

        public void Delete()
        {
            RealtimeModel.Delete();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
        }
    }
}
