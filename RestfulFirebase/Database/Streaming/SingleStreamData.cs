using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class SingleStreamData : StreamData
    {
        public string Data { get; }

        internal SingleStreamData(string data)
        {

        }
    }
}
