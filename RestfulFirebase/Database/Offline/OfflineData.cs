using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData
    {
        public string SyncedData { get; internal set; }
        public string LocalData { get; internal set; }

        public static OfflineData Parse(string data)
        {
            var syncedData = Helpers.BlobGetValue(data, "sync");
            var localData = Helpers.BlobGetValue(data, "local");
            return new OfflineData(syncedData, localData);
        }

        public OfflineData(string syncedData, string localData)
        {
            SyncedData = syncedData;
            LocalData = localData;
        }

        public override string ToString()
        {
            var data = Helpers.BlobSetValue("", "sync", SyncedData);
            data = Helpers.BlobSetValue(data, "local", LocalData);
            return data;
        }
    }
}
