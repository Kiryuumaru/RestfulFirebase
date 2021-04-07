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

        public void SetData(string path, PrimitiveBlob data)
        {
            App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabaseDataRoot, path), data.Blob);
        }

        public PrimitiveBlob GetData(string path)
        {
            var data = App.LocalDatabase.Get(Helpers.CombineUrl(OfflineDatabaseDataRoot, path));
            return PrimitiveBlob.CreateFromBlob(data);
        }

        public IEnumerable<PrimitiveBlob> GetAll(string path)
        {
            foreach (var data in App.LocalDatabase.GetAll(Helpers.CombineUrl(OfflineDatabaseDataRoot, path)))
            {
                yield return PrimitiveBlob.CreateFromBlob(data);
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
