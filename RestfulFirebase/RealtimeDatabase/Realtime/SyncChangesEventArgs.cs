using System;

namespace RestfulFirebase.RealtimeDatabase.Realtime;

/// <summary>
/// Event arguments for data evaluated invokes.
/// </summary>
public class SyncChangesEventArgs : EventArgs
{
    /// <summary>
    /// Gets <c>true</c> whether the node is fully synced; otherwise <c>false</c>.
    /// </summary>
    public bool IsSynced => TotalDataCount == SyncedDataCount;

    /// <summary>
    /// Gets the total data cached of the instance.
    /// </summary>
    public int TotalDataCount { get; }

    /// <summary>
    /// Gets the total synced data cached of node instance.
    /// </summary>
    public int SyncedDataCount { get; }

    internal SyncChangesEventArgs(int totalDataCount, int syncedDataCount)
    {
        TotalDataCount = totalDataCount;
        SyncedDataCount = syncedDataCount;
    }
}
