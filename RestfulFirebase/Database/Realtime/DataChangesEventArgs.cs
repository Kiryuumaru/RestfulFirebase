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

        public int TotalDataCount { get; }

        public int SyncedDataCount { get; }

        public string Uri => (string.IsNullOrEmpty(Path) ? BaseUri : Utils.UrlCombine(BaseUri, Path)).Trim().Trim('/');

        public DataChangesEventArgs(string baseUri, string path, int totalDataCount, int syncedDataCount)
        {
            BaseUri = baseUri.Trim().Trim('/');
            Path = path.Trim().Trim('/');
            TotalDataCount = totalDataCount;
            SyncedDataCount = syncedDataCount;
        }
    }
}
