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
        private static readonly string BlobPath = Helpers.CombineUrl(Root, "blob");
        private static readonly string ChangesPath = Helpers.CombineUrl(Root, "changes");
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
                App.LocalDatabase.Set(combined, data);
            }

            private void Delete(params string[] path)
            {
                App.LocalDatabase.Delete(Helpers.CombineUrl(path));
            }

            public RestfulFirebaseApp App { get; }

            public string Path { get; }

            public bool HasChanges { get; private set; }

            public bool Exist => Short != null;

            public string Short
            {
                get => Get(ShortPath, Path);
                private set => Set(value, ShortPath, Path);
            }

            public string Long
            {
                get => Get(LongPath, Short);
                private set => Set(value, LongPath, Short);
            }

            public string Blob
            {
                get => Get(BlobPath, Short);
                set => Set(value, BlobPath, Short);
            }

            public OfflineChanges Changes
            {
                get
                {
                    if (Short == null) return OfflineChanges.None;
                    var changes = Get(ChangesPath, Short);
                    switch (changes)
                    {
                        case "1":
                            return OfflineChanges.Set;
                        case "2":
                            return OfflineChanges.Delete;
                        default:
                            return OfflineChanges.None;
                    }
                }
                set
                {
                    switch (value)
                    {
                        case OfflineChanges.Set:
                            Set("1", ChangesPath, Short);
                            break;
                        case OfflineChanges.Delete:
                            Set("2", ChangesPath, Short);
                            break;
                        default:
                            Set("0", ChangesPath, Short);
                            break;
                    }
                }
            }

            public OfflineSyncStrategy SyncStrategy
            {
                get
                {
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
                var shortPath = Short;
                Delete(ShortPath, Path);
                Delete(LongPath, shortPath);
                Delete(BlobPath, shortPath);
                Delete(ChangesPath, shortPath);
                Delete(SyncStratPath, shortPath);
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
