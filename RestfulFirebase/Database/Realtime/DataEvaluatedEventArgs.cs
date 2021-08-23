using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// Event arguments for data evaluated invokes.
    /// </summary>
    public class DataEvaluatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets <c>true</c> whether the node is fully synced; otherwise <c>false</c>.
        /// </summary>
        public bool IsSynced => TotalDataCount == SyncedDataCount;

        /// <summary>
        /// Gets the total data cached of the instance.
        /// </summary>
        public int TotalDataCount { get; private set; }

        /// <summary>
        /// Gets the total synced data cached of node instance.
        /// </summary>
        public int SyncedDataCount { get; private set; }

        internal DataEvaluatedEventArgs(int totalDataCount, int syncedDataCount)
        {
            TotalDataCount = totalDataCount;
            SyncedDataCount = syncedDataCount;
        }
    }
}
