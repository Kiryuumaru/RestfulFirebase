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
        private readonly string OfflineDatabaseLastModifiedPath = Helpers.CombineUrl(OfflineDatabaseRoot, "modified");
        private readonly string OfflineDatabaseLocalDataPath = Helpers.CombineUrl(OfflineDatabaseRoot, "local");
        private readonly string OfflineDatabaseSyncDataPath = Helpers.CombineUrl(OfflineDatabaseRoot, "sync");

        public RestfulFirebaseApp App { get; }

        public SmallDateTime LastModified
        {
            get => Helpers.DecodeSmallDateTime(App.LocalDatabase.Get(OfflineDatabaseLastModifiedPath), SmallDateTime.MinValue);
            private set => App.LocalDatabase.Set(OfflineDatabaseLastModifiedPath, Helpers.EncodeSmallDateTime(value));
        }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public string GetLocalData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            if (string.IsNullOrEmpty(data)) return null;
            return data;
        }

        public string GetSyncData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path));
            if (string.IsNullOrEmpty(data)) return null;
            return data;
        }

        public void SetLocalData(string path, string data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path), data);
            LastModified = SmallDateTime.UtcNow;
        }

        public void SetSyncData(string path, string data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path), data);
            LastModified = SmallDateTime.UtcNow;
        }

        public void DeleteLocalData(string path)
        {
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            LastModified = SmallDateTime.UtcNow;
        }

        public void DeleteSyncData(string path)
        {
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path));
            LastModified = SmallDateTime.UtcNow;
        }

        public IEnumerable<string> GetAllLocal(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path)))
            {
                if (string.IsNullOrEmpty(data)) continue;
                yield return data;
            }
        }

        public IEnumerable<string> GetAllSync(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseSyncDataPath, path)))
            {
                if (string.IsNullOrEmpty(data)) continue;
                yield return data;
            }
        }
    }
}
