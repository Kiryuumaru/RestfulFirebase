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
        private static readonly string LocalBlobPath = Helpers.CombineUrl(Root, "local_blob");
        private static readonly string SyncBlobPath = Helpers.CombineUrl(Root, "sync_blob");
        private static readonly string SyncStratPath = Helpers.CombineUrl(Root, "sync_strat");

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
            public bool HasLocalBlobChanges { get; private set; }
            public bool HasSyncBlobChanges { get; private set; }
            public bool HasCurrentBlobChanges { get; private set; }

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

            public string LocalBlob
            {
                get
                {
                    if (!Exist) return null;
                    return Get(LocalBlobPath, Short);
                }
                set
                {
                    if (!Exist) Create();
                    if (Get(LocalBlobPath, Short) != value) HasLocalBlobChanges = true;
                    var oldCurr = CurrentBlob;
                    Set(value, LocalBlobPath, Short);
                    if (oldCurr != CurrentBlob) HasCurrentBlobChanges = true;
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
                    if (Get(SyncBlobPath, Short) != value) HasSyncBlobChanges = true;
                    var oldCurr = CurrentBlob;
                    Set(value, SyncBlobPath, Short);
                    if (oldCurr != CurrentBlob) HasCurrentBlobChanges = true;
                }
            }

            public string CurrentBlob
            {
                get
                {
                    if (!Exist) return null;
                    var local = LocalBlob;
                    var sync = SyncBlob;
                    return local == null ? sync : local;
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
                var oldCurr = CurrentBlob;
                var shortPath = Short;
                Set(null, ShortPath, Path);
                Set(null, LongPath, shortPath);
                Set(null, LocalBlobPath, shortPath);
                Set(null, SyncBlobPath, shortPath);
                Set(null, SyncStratPath, shortPath);
                if (oldCurr != CurrentBlob) HasCurrentBlobChanges = true;
                return true;
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
