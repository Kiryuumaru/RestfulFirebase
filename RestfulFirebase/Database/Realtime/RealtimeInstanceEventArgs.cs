using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class RealtimeInstanceEventArgs : EventArgs
    {
        public RealtimeInstance RealtimeInstance { get; }

        internal RealtimeInstanceEventArgs(RealtimeInstance realtimeInstance)
        {
            RealtimeInstance = realtimeInstance;
        }
    }
}
