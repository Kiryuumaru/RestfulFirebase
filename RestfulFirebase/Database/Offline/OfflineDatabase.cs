using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineDatabase
    {
        private const string OfflineDatabaseRoot = "offlineDatabase";
        private readonly string OfflineDatabaseModifiedPath = Helpers.CombineUrl(OfflineDatabaseRoot, "mod");
        private readonly string OfflineDatabaseLocalDataPath = Helpers.CombineUrl(OfflineDatabaseRoot, "local");
        private readonly string OfflineDatabaseSyncDataPath = Helpers.CombineUrl(OfflineDatabaseRoot, "sync");

        public RestfulFirebaseApp App { get; }

        public SmallDateTime LastModified
        {
            get => Helpers.DecodeSmallDateTime(App.LocalDatabase.Get(OfflineDatabaseModifiedPath), SmallDateTime.MinValue);
            private set => App.LocalDatabase.Set(OfflineDatabaseModifiedPath, Helpers.EncodeSmallDateTime(value));
        }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public OfflineData GetLocalData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            return new OfflineData(data);
        }

        public OfflineData GetSyncData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path));
            return new OfflineData(data);
        }

        public bool SetLocalData(string path, OfflineData data)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path), data.Blob);
            LastModified = SmallDateTime.UtcNow;
            return dataOld != data.Blob;
        }

        public bool SetSyncData(string path, OfflineData data)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path), data.Blob);
            LastModified = SmallDateTime.UtcNow;
            return dataOld != data.Blob;
        }

        public bool DeleteLocalData(string path)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            LastModified = SmallDateTime.UtcNow;
            return dataOld != null;
        }

        public bool DeleteSyncData(string path)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path));
            LastModified = SmallDateTime.UtcNow;
            return dataOld != null;
        }

        public IEnumerable<OfflineData> GetAllLocal(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path)))
            {
                if (string.IsNullOrEmpty(data)) continue;
                var offlineData = new OfflineData(data);
                if (offlineData == null) continue;
                yield return offlineData;
            }
        }

        public IEnumerable<OfflineData> GetAllSync(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path)))
            {
                if (string.IsNullOrEmpty(data)) continue;
                var offlineData = new OfflineData(data);
                if (offlineData == null) continue;
                yield return offlineData;
            }
        }
    }
}
