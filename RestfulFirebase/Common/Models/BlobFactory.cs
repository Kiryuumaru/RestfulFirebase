using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class BlobFactory
    {
        public Func<(string Value, string Tag), bool> Set { get; private set; }
        public Func<string> Get { get; private set; }
        public BlobFactory(Func<(string Value, string Tag), bool> set, Func<string> get)
        {
            Set = set;
            Get = get;
        }
    }
}
