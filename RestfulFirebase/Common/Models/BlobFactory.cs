using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class BlobFactory
    {
        private Func<(string blob, string tag), bool> set;
        private Func<string, string> get;

        public BlobFactory(Func<(string Blob, string Tag), bool> set, Func<string, string> get)
        {
            this.set = set;
            this.get = get;
        }

        public bool Set(string blob, string tag = null)
        {
            return set.Invoke((blob, tag));
        }

        public string Get(string tag = null)
        {
            return get.Invoke(tag);
        }
    }
}
