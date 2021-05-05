using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamData
    {
        internal StreamData()
        {

        }
    }

    public class SingleStreamData : StreamData
    {
        public string Data { get; }

        internal SingleStreamData(string data)
        {
            Data = data;
        }
    }

    public class MultiStreamData : StreamData
    {
        public Dictionary<string, StreamData> Data { get; }

        internal MultiStreamData(Dictionary<string, StreamData> data)
        {
            Data = data;
        }
    }
}
