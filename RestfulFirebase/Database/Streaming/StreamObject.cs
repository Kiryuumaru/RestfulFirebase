using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject
    {
        public string[] Path { get; }

        public string Data { get; }

        internal StreamObject(string[] path, string data)
        {
            Path = path;
            Data = data;
        }
    }
}
