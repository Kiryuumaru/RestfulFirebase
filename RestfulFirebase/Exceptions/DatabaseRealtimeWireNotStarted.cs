using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when operation requires realtime wire to start.
    /// </summary>
    public class DatabaseRealtimeWireNotStarted : DatabaseException
    {
        internal DatabaseRealtimeWireNotStarted(string methodName)
            : base(methodName + " operation requires realtime wire to start.")
        {

        }
    }
}
