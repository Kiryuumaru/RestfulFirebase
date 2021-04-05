using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineDatabase
    {
        private const string OfflineDatabaseRoot = "offlineDatabase";
        private readonly string OfflineDatabaseDataRoot = Helpers.CombineUrl(OfflineDatabaseRoot, "data");

        public RestfulFirebaseApp App { get; }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public void SetSyncData(string path, string data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "sync"), data.ToString());
        }

        public void SetLocalData(string path, string data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "local"), data.ToString());
        }

        public string GetSyncData(string path)
        {
            return App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "sync"));
        }

        public string GetLocalData(string path)
        {
            return App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "local"));
        }

        public IEnumerable<OfflineData> GetAll(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseDataRoot, path)))
            {
                yield return OfflineData.Parse(data);
            }
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            return App.LocalDatabase.GetSubPaths(Helpers.CombineUrl(OfflineDatabaseDataRoot, path));
        }

        private void AddPathQueue(IEnumerable<string> paths)
        {

        }
    }
}
