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
        private const string Root = "offline_database";
        private readonly string DataPath = Helpers.CombineUrl(Root, "data");
        private readonly string ActiveSyncPath = Helpers.CombineUrl(Root, "active_sync");
        private readonly string PassiveSyncPath = Helpers.CombineUrl(Root, "passive_sync");

        public RestfulFirebaseApp App { get; }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public string GetData(params string[] path)
        {
            if (path == null) return null;
            if (path.Length == 0) return null;
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(DataPath, path[0]));
            if (data == null) return null;
            for (int i = 1; i < path.Length; i++)
            {
                data = Helpers.BlobGetValue(data, path[i], null);
                if (data == null) return null;
            }
            return data;
        }







































        public OfflineData GetLocalData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            if (data == null) return null;
            return new OfflineData(data);
        }

        public string GetSyncBlob(string path)
        {
            return App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseSyncBlobPath, path));
        }

        public bool SetLocalData(string path, OfflineData data)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path), data.Data);
            return dataOld != data.Data;
        }

        public bool SetSyncBlob(string path, string blob)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseSyncBlobPath, path), blob);
            return dataOld != blob;
        }

        public bool DeleteLocalData(string path)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            return dataOld != null;
        }

        public bool DeleteSyncBlob(string path)
        {
            var dataOld = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path));
            App.LocalDatabase.Delete(Helpers.CombineUrl(OfflineDatabaseSyncBlobPath, path));
            return dataOld != null;
        }

        public IEnumerable<OfflineData> GetAllLocalData(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseLocalDataPath, path)))
            {
                if (data == null) continue;
                yield return new OfflineData(data);
            }
        }

        public IEnumerable<string> GetAllSyncBlob(string path)
        {
            foreach (var blob in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseSyncBlobPath, path)))
            {
                if (blob == null) continue;
                yield return blob;
            }
        }
    }
}
