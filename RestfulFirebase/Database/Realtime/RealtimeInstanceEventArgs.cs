using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// Event arguments for realtime model event invokes.
    /// </summary>
    public class RealtimeInstanceEventArgs : EventArgs
    {
        /// <summary>
        /// The realtime instance of the model.
        /// </summary>
        public RealtimeInstance RealtimeInstance { get; }

        internal RealtimeInstanceEventArgs(RealtimeInstance realtimeInstance)
        {
            RealtimeInstance = realtimeInstance;
        }
    }
}
