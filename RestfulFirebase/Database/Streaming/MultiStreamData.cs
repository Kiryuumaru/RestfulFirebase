using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class MultiStreamData : StreamData
    {
        public Dictionary<string, StreamData> Data { get; }

        internal MultiStreamData(Dictionary<string, StreamData> data)
        {
            Data = data;
        }
    }
}
