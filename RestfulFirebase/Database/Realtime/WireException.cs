using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// The exception class for realtime wire errors.
    /// </summary>
    public class WireException : Exception
    {
        /// <summary>
        /// The Uri of the exception
        /// </summary>
        public string Uri { get; }

        internal WireException(string uri, Exception exception)
            : base("An error occured with realtime wire: " + uri, exception)
        {
            Uri = uri;
        }
    }
}
