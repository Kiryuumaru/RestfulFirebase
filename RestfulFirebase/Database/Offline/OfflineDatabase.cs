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

        public void Set(string path, OfflineData data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path), data.ToString());
        }

        public OfflineData Get(string path)
        {
            return OfflineData.Parse(App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path)));
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
