using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject
    {
        public string Key { get; }

        public string Data { get; }

        internal StreamObject(string key, string data)
        {
            Key = key;
            Data = data;
        }
    }
}
