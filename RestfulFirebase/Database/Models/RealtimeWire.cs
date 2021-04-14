using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class RealtimeWire
    {
        public const string ModifiedKey = "_m";
        public const string RealtimeInitializeTag = "realtime_init";
        public const string SyncTag = "sync";
        public const string RevertTag = "revert";

        private Action startRealtime;
        private Action stopRealtime;
        private Action<StreamObject> consumeStream;

        public string Path { get; private set; }
        public FirebaseQuery Query { get; }

        public RealtimeWire(FirebaseQuery query, Action startRealtime, Action stopRealtime, Action<StreamObject> consumeStream)
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

        public void ConsumeStream(StreamObject streamObject)
        {
            consumeStream.Invoke(streamObject);
        }
    }
}
