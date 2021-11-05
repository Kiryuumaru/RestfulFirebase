using RestfulFirebase.Utilities;
using System;

namespace RestfulFirebase.Local
{
    /// <summary>
    /// Event arguments for data changes invokes.
    /// </summary>
    public class DataChangesEventArgs : EventArgs
    {
        /// <summary>
        /// The path of the data changes.
        /// </summary>
        public string[] Path { get; }

        internal DataChangesEventArgs(string[] path)
        {
            Path = path;
        }
    }
}
