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
        #region Properties


        #endregion

        #region Initializers


        #endregion

        #region Methods


        #endregion
        private const string Root = "offdb";
        private static readonly string ShortPath = Helpers.CombineUrl(Root, "short");
        private static readonly string LongPath = Helpers.CombineUrl(Root, "long");
        private static readonly string SyncBlobPath = Helpers.CombineUrl(Root, "blob");
        private static readonly string ChangesPath = Helpers.CombineUrl(Root, "changes");
        private static readonly string SyncStratPath = Helpers.CombineUrl(Root, "strat");
        private static readonly string SubDataPath = Helpers.CombineUrl(Root, "subdata");

        public class Data
        {
            #region Properties

            public RestfulFirebaseApp App { get; }

            public string Path { get; }

            public bool HasChanges { get; private set; }

            public bool HasBlobChanges { get; private set; }

            public bool HasLatestBlobChanges { get; private set; }

            public bool Exist => Short != null;

            public string Short
            {
                get => Get(ShortPath, Path);
                private set => Set(value, ShortPath, Path);
            }

            public string Long
            {
                get
                {
                    if (!Exist) return null;
                    return Get(LongPath, Short);
                }
                private set
                {
                    if (!Exist) Create();
                    Set(value, LongPath, Short);
                }
            }

            public string SyncBlob
            {
                get
                {
                    if (!Exist) return null;
                    return Get(SyncBlobPath, Short);
                }
                set
                {
                    if (!Exist) Create();
                    var oldBlob = SyncBlob;
                    var oldLatestBlob = LatestBlob;
                    Set(value, SyncBlobPath, Short);
                    if (oldBlob != SyncBlob) HasBlobChanges = true;
                    if (oldLatestBlob != LatestBlob) HasLatestBlobChanges = true;
                }
            }

            public OfflineChanges Changes
            {
                get
                {
                    if (!Exist) return null;
                    return OfflineChanges.Parse(Get(ChangesPath, Short));
                }
                set
                {
                    if (!Exist) Create();
                    var oldLatestBlob = LatestBlob;
                    Set(value?.ToData(), ChangesPath, Short);
                    if (oldLatestBlob != LatestBlob) HasLatestBlobChanges = true;
                }
            }

            public string LatestBlob
            {
                get
                {
                    if (!Exist) return null;
                    var sync = SyncBlob;
                    var changes = Changes;
                    return changes == null ? sync : changes.Blob;
                }
            }

            public OfflineSyncStrategy SyncStrategy
            {
                get
                {
                    if (!Exist) return OfflineSyncStrategy.None;
                    var strat = Get(SyncStratPath, Short);
                    switch (strat)
                    {
                        case "1":
                            return OfflineSyncStrategy.Active;
                        case "2":
                            return OfflineSyncStrategy.Passive;
                        default:
                            return OfflineSyncStrategy.None;
                    }
                }
                set
                {
                    if (!Exist) Create();
                    switch (value)
                    {
                        case OfflineSyncStrategy.Active:
                            Set("1", SyncStratPath, Short);
                            break;
                        case OfflineSyncStrategy.Passive:
                            Set("2", SyncStratPath, Short);
                            break;
                        default:
                            Set("0", SyncStratPath, Short);
                            break;
                    }
                }
            }

            #endregion

            #region Initializers

            public Data(RestfulFirebaseApp app, string path)
            {
                App = app;
                Path = path;
            }

            #endregion

            #region Methods

            private string GetUniqueShort()
            {
                string uid = null;
                while (uid == null)
                {
                    uid = Helpers.GenerateUID(5, Helpers.Base64Charset);
                    var path = Get(LongPath, uid);
                    if (path != null) uid = null;
                }
                return uid;
            }

            private string Get(params string[] path)
            {
                return App.LocalDatabase.Get(Helpers.CombineUrl(path));
            }

            private void Set(string data, params string[] path)
            {
                var combined = Helpers.CombineUrl(path);
                if (App.LocalDatabase.Get(combined) != data) HasChanges = true;
                if (data == null) App.LocalDatabase.Delete(combined);
                else App.LocalDatabase.Set(combined, data);
            }

            public bool Create()
            {
                if (Exist) return false;
                Short = GetUniqueShort();
                Long = Path;
                return true;
            }

            public bool Delete()
            {
                if (!Exist) return false;
                var oldCurr = LatestBlob;
                var shortPath = Short;
                Set(null, ShortPath, Path);
                Set(null, LongPath, shortPath);
                Set(null, SyncBlobPath, shortPath);
                Set(null, ChangesPath, shortPath);
                Set(null, SyncStratPath, shortPath);
                if (oldCurr != LatestBlob) HasLatestBlobChanges = true;
                return true;
            }

            #endregion
        }

        public class DataCollection
        {
            #region Properties

            public RestfulFirebaseApp App { get; }

            public string Path { get; }

            public Data this[string key]
            {
                get
                {

                }
                set
                {

                }
            }

            #endregion

            #region Initializers

            public DataCollection(RestfulFirebaseApp app, string path)
            {
                App = app;
                Path = path;
            }

            #endregion

            #region Methods

            public void s()
            {
                var datas = new List<Data>();
                var subDatas = Helpers.DeserializeString(App.LocalDatabase.Get(Helpers.CombineUrl(Path)));
                foreach (var subData in subDatas)
                {
                    var data = new Data(App, subData);
                    if (data.Exist) datas.Add(data);
                }
                return datas;
            }

            #endregion
        }

        public RestfulFirebaseApp App { get; }

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public Data GetData(string path)
        {
            return new Data(App, path);
        }

        public DataCollection GetDataCollection(string path)
        {
            return new DataCollection(App, path);
        }

        public void Flush()
        {
            foreach (var path in App.LocalDatabase.GetSubPaths(Root))
            {
                App.LocalDatabase.Delete(path);
            }
        }
    }
}
