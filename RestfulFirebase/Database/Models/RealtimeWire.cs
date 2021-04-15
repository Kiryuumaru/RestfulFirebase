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

        public string Path { get; private set; }
        public FirebaseQuery Query { get; }
        public bool HasFirstStream { get; private set; } = false;

        public RealtimeWire(FirebaseQuery query, Action startRealtime, Action stopRealtime, Func<StreamObject, bool> consumeStream)
        {
            Path = query.GetAbsolutePath();
            Query = query;
            this.startRealtime = startRealtime;
            this.stopRealtime = stopRealtime;
            this.consumeStream = consumeStream;
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
