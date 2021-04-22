using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class RealtimeWire
    {
        private Action startRealtime;
        private Action stopRealtime;
        private Func<StreamObject, bool> consumeStream;

        public FirebaseQuery Query { get; }
        public string Path { get; }
        public bool HasFirstStream { get; private set; } = false;

        public RealtimeWire(FirebaseQuery query, Action startRealtime, Action stopRealtime, Func<StreamObject, bool> consumeStream)
        {
            Query = query;
            Path = query.GetAbsolutePath();
            this.startRealtime = startRealtime;
            this.stopRealtime = stopRealtime;
            this.consumeStream = consumeStream;
        }

        public ChildQuery Child(string path)
        {
            return new ChildQuery(Query.App, Query, () => path);
        }

        public void StartRealtime()
        {
            startRealtime.Invoke();
        }

        public void StopRealtime()
        {
            stopRealtime.Invoke();
        }

        public bool ConsumeStream(StreamObject streamObject)
        {
            var ret = consumeStream.Invoke(streamObject);
            HasFirstStream = true;
            return ret;
        }
    }
}
