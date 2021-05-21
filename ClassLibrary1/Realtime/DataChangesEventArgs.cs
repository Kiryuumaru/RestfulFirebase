using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class DataChangesEventArgs : EventArgs
    {
        public int TotalDataCount { get; }
        public int SyncedDataCount { get; }

        public DataChangesEventArgs(int totalDataCount, int syncedDataCount)
        {
            TotalDataCount = totalDataCount;
            SyncedDataCount = syncedDataCount;
        }
    }
}
