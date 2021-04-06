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
        private readonly string OfflineDatabaseDataRoot = Helpers.CombineUrl(OfflineDatabaseRoot, "data");

        public RestfulFirebaseApp App { get; }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public void SetSyncData(string path, PrimitiveData data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "sync"), data.Data);
        }

        public void SetLocalData(string path, PrimitiveData data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "local"), data.Data);
        }

        public PrimitiveData GetSyncData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "sync"));
            return PrimitiveData.CreateFromData(data);
        }

        public PrimitiveData GetLocalData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path, "local"));
            return PrimitiveData.CreateFromData(data);
        }

        public IEnumerable<PrimitiveData> GetAll(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseDataRoot, path)))
            {
                yield return PrimitiveData.CreateFromData(data);
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
