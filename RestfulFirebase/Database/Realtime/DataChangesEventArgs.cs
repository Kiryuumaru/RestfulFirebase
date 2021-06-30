using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// Event arguments for data changes invokes.
    /// </summary>
    public class DataChangesEventArgs : EventArgs
    {
        /// <summary>
        /// The base uri of the data changes.
        /// </summary>
        public string BaseUri { get; }

        /// <summary>
        /// The path of the data changes.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The uri of the data changes.
        /// </summary>
        public string Uri { get; }

        internal DataChangesEventArgs(string baseUri, string path)
        {
            BaseUri = baseUri.Trim().Trim('/');
            Path = path.Trim().Trim('/');
            Uri = (string.IsNullOrEmpty(Path) ? BaseUri : Utils.UrlCombine(BaseUri, Path)).Trim().Trim('/');
        }
    }
}
