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
        private Action<StreamObject> consumeStream;

        public string Path { get; private set; }
        public FirebaseQuery Query { get; }

        public RealtimeWire(FirebaseQuery query, Action startRealtime, Action<StreamObject> consumeStream)
        {
            Path = query.GetAbsolutePath();
            Query = query;
            this.startRealtime = startRealtime;
            this.consumeStream = consumeStream;
        }

        public void StartRealtime()
        {
            startRealtime.Invoke();
        }

        public void ConsumeStream(StreamObject streamObject)
        {
            consumeStream.Invoke(streamObject);
        }
    }
}
