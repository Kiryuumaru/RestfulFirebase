using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class DataChangesEventArgs : EventArgs
    {
        public string BaseUri { get; }

        public string Path { get; }

        public string Uri { get; }

        public DataChangesEventArgs(string baseUri, string path)
        {
            BaseUri = baseUri.Trim().Trim('/');
            Path = path.Trim().Trim('/');
            Uri = (string.IsNullOrEmpty(Path) ? BaseUri : Utils.UrlCombine(BaseUri, Path)).Trim().Trim('/');
        }
    }
}
