using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class DataChangesEventArgs : EventArgs
    {
        public string BaseUri { get; }

        public string Path { get; }

        public string Uri => (string.IsNullOrEmpty(Path) ? BaseUri : Utils.CombineUrl(BaseUri, Path));

        public DataChangesEventArgs(string baseUri, string path)
        {
            BaseUri = baseUri;
            Path = path;
        }
    }
}
