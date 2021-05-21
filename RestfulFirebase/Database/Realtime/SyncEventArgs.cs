using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class SyncEventArgs : EventArgs
    {
        public int TotalDataCount { get; }
        public int SyncedDataCount { get; }

        public SyncEventArgs(int totalDataCount, int syncedDataCount)
        {
            TotalDataCount = totalDataCount;
            SyncedDataCount = syncedDataCount;
        }
    }
}
