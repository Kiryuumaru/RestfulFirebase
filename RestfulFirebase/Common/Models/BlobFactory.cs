using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class BlobFactory
    {
        public Action<(string Value, string Tag)> Set { get; private set; }
        public Func<string> Get { get; private set; }
        public BlobFactory(Action<(string Value, string Tag)> set, Func<string> get)
        {
            Set = set;
            Get = get;
        }
    }
}
