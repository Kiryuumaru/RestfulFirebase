using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamObject<T>
    {
        public string Key { get; }

        public T Object { get; }

        internal StreamObject(string key, T obj)
        {
            Key = key;
            Object = obj;
        }
    }
}
