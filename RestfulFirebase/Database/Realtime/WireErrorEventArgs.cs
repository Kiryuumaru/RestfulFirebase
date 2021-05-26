using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class WireErrorEventArgs : EventArgs
    {
        public string Uri { get; }

        public Exception Exception { get; }

        public WireErrorEventArgs(string uri, Exception exception)
        {
            Uri = uri;
            Exception = exception;
        }
    }
}
