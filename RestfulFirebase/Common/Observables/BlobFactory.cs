using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public class BlobFactory
    {
        private Func<(string blob, string tag), bool> set;
        private Func<(string defaultBlob, string tag), string> get;

        public BlobFactory(
            Func<(string blob, string tag), bool> set,
            Func<(string defaultBlob, string tag), string> get)
        {
            this.set = set;
            this.get = get;
        }

        public bool Set(string blob, string tag = null) => set((blob, tag));

        public string Get(string defaultBlob = default, string tag = null) => get((defaultBlob, tag));
    }
}
