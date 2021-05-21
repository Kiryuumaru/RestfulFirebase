using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject
    {
        public StreamData Data { get; }

        public string Uri { get; }

        internal StreamObject(StreamData data, string uri)
        {
            Data = data;
            Uri = uri;
        }
    }
}
