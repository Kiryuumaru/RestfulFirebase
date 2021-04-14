using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject
    {
        public string Data { get; }

        public string[] Path { get; }

        internal StreamObject(string data, params string[] path)
        {
            Data = data;
            Path = path;
        }
    }
}
