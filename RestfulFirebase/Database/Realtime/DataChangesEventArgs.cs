using RestfulFirebase.Utilities;
using System;

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
            Uri = (string.IsNullOrEmpty(Path) ? BaseUri : UrlUtilities.Combine(BaseUri, Path)).Trim().Trim('/');
        }
    }
}
