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
        #region Helper Classes

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

            public IEnumerable<Data> SubDatas
            {
                get
                {
                    if (!Exist) Create();
                    var deserialized = Helpers.DeserializeString(Get(SubDataPath, Short));
                    if (deserialized == null) return new Data[0];
                    var subDatas = new List<Data>();
                    foreach (var shortPath in deserialized)
                    {
                        var longPath = Get(LongPath, shortPath);
                        if (longPath == null) continue;
                        subDatas.Add(new Data(App, longPath));
                    }
                    return subDatas;
                }
                private set
                {
                    if (!Exist) Create();
                    var serialized = Helpers.SerializeString(value.Select(i => i.Short).ToArray());
                    Set(serialized, SubDataPath, Short);
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
                Set(null, SubDataPath, shortPath);
                if (oldCurr != LatestBlob) HasLatestBlobChanges = true;
                return true;
            }

            public void SetSubData(string key)
            {
                var subDatas = new List<Data>(SubDatas);
                if (subDatas.Any(i => Helpers.CombineUrl(i.Path) == Helpers.CombineUrl(Path, key))) subDatas.Add(new Data(App, Helpers.CombineUrl(Path, key)));
                SubDatas = subDatas;
            }

            public void DeleteSubData(string key)
            {
                var subDatas = new List<Data>(SubDatas);
                subDatas.RemoveAll(i => Helpers.CombineUrl(i.Path) == Helpers.CombineUrl(Path, key));
                SubDatas = subDatas;
            }

            #endregion
        }

        #endregion

        #region Properties

        private const string Root = "offdb";
        private static readonly string ShortPath = Helpers.CombineUrl(Root, "short");
        private static readonly string LongPath = Helpers.CombineUrl(Root, "long");
        private static readonly string SyncBlobPath = Helpers.CombineUrl(Root, "blob");
        private static readonly string ChangesPath = Helpers.CombineUrl(Root, "changes");
        private static readonly string SyncStratPath = Helpers.CombineUrl(Root, "strat");
        private static readonly string SubDataPath = Helpers.CombineUrl(Root, "subdata");

        public RestfulFirebaseApp App { get; }

        public event Action<OfflineChanges> OnChanges;

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        #endregion

        #region Methods

        public Data GetData(string path)
        {
            return new Data(App, path);
        }

        public void Flush()
        {
            foreach (var path in App.LocalDatabase.GetSubPaths(Root))
            {
                App.LocalDatabase.Delete(path);
            }
        }

        #endregion
    }
}
