using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeHolder<T>
        where T : IRealtimeModel
    {
        public T RealtimeModel { get; private set; }
        public IFirebaseQuery Query { get; private set; }
        public bool InvokeSetFirst { get; private set; }

        internal RealtimeHolder(T realtime, IFirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeModel = realtime;
            Query = query;
            InvokeSetFirst = invokeSetFirst;
        }

        public void Start()
        {
            RealtimeModel.SetRealtime(Query, InvokeSetFirst);
        }

        public void Delete()
        {
            RealtimeModel.Delete();
        }
    }
}
