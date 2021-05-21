using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class DataChangesEventArgs : EventArgs
    {
        public string Uri { get; }

        public DataChangesEventArgs(string uri)
        {
            Uri = uri;
        }
    }
}
