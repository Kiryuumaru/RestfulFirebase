using System;
using System.Collections.Generic;
using System.Linq;
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

        public StreamObject Skip(int count)
        {
            return new StreamObject(Path.Skip(count).ToArray(), Data);
        }
    }
}
