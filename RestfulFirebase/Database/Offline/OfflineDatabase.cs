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
        private static readonly string ShortPath = Helpers.CombineUrl(Root, "short");
        private static readonly string LongPath = Helpers.CombineUrl(Root, "long");
        private static readonly string ChildrenPath = Helpers.CombineUrl(Root, "child");
        private static readonly string SyncBlobPath = Helpers.CombineUrl(Root, "blob");
        private static readonly string ChangesPath = Helpers.CombineUrl(Root, "changes");
        private static readonly string SyncStratPath = Helpers.CombineUrl(Root, "strat");

        public class Data
        {
            private string GetUniqueShort()
            {
                string uid = null;
                while (uid == null)
                {
                    uid = Helpers.GenerateUID();
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

            public IEnumerable<Data> Children
            {
                get
                {
                    if (!Exist) return null;
                    var data = Get(ChildrenPath, Short);
                    var deserialized = Helpers.DeserializeString(data);
                    var shorts = deserialized == null ? new List<string>() : new List<string>(deserialized);
                    var datas = new List<Data>();
                    foreach (var shortPath in shorts)
                    {
                        var longPath = Get(LongPath, shortPath);
                        if (longPath != null) datas.Add(new Data(App, longPath));
                    }
                    return datas;
                }
                private set
                {
                    if (!Exist) Create();
                    if (value == null) Set(null, ChildrenPath, Short);
                    else Set(Helpers.SerializeString(value.Select(i => i.Short).ToArray()), ChildrenPath, Short);
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

            public Data(RestfulFirebaseApp app, string path)
            {
                App = app;
                Path = path;
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
                Set(null, ChildrenPath, shortPath);
                Set(null, SyncBlobPath, shortPath);
                Set(null, ChangesPath, shortPath);
                Set(null, SyncStratPath, shortPath);
                if (oldCurr != LatestBlob) HasLatestBlobChanges = true;
                return true;
            }

            public void AddChild(string key)
            {
                if (!Exist) Create();
                var path = Helpers.CombineUrl(Path, key);
                var value = new List<Data>(Children);
                var data = new Data(App, path);
                data.Create();
                value.Add(data);
                Children = value;
            }

            public void DeleteChild(string key)
            {
                if (!Exist) Create();
                var path = Helpers.CombineUrl(Path, key);
                var value = new List<Data>(Children);
                if (value.Any(i => i.Path == path))
                {
                    value.RemoveAll(i => i.Path == path);
                    Children = value;
                }
            }
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
    }
}
