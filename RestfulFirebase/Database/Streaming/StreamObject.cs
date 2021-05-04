using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject
    {
        public StreamData Object { get; }

        public string[] Path { get; }

        internal StreamObject(StreamData data, params string[] path)
        {
            Object = data;
            Path = path;
        }
    }
}
