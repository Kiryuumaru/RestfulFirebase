using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class DataNode
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string Path { get; }

        public string Key => Helpers.SeparateUrl(Path).Last();

        public bool Exist => Get(OfflineDatabase.ShortPath, Path) != null;

        public bool HasBlobChanges { get; private set; }

        public string Short
        {
            get
            {
                var shortPath = Get(OfflineDatabase.ShortPath, Path);
                if (shortPath == null)
                {
                    shortPath = GetUniqueShort();
                    App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabase.ShortPath, Path), shortPath);
                    App.LocalDatabase.Set(Helpers.CombineUrl(OfflineDatabase.LongPath, shortPath), Path);
                }
                return shortPath;
            }
            protected set => Set(value, OfflineDatabase.ShortPath, Path);
        }

        public string Long
        {
            get => Get(OfflineDatabase.LongPath, Short);
            protected set => Set(value, OfflineDatabase.LongPath, Short);
        }

        public string Sync
        {
            get => Get(OfflineDatabase.SyncBlobPath, Short);
            set
            {
                var oldBlob = Blob;
                Set(value, OfflineDatabase.SyncBlobPath, Short);
                if (oldBlob != Blob) HasBlobChanges = true;
            }
        }

        public DataChanges Changes
        {
            get => DataChanges.Parse(Get(OfflineDatabase.ChangesPath, Short));
            set
            {
                var oldBlob = Blob;
                Set(value?.ToData(), OfflineDatabase.ChangesPath, Short);
                if (oldBlob != Blob) HasBlobChanges = true;
            }
        }

        public string Blob
        {
            get
            {
                if (!Exist) return null;
                var sync = Sync;
                var changes = Changes;
                return changes == null ? sync : changes.Blob;
            }
        }

        public SyncStrategy SyncStrategy
        {
            get
            {
                switch (Get(OfflineDatabase.SyncStratPath, Short))
                {
                    case "1":
                        return SyncStrategy.Active;
                    case "2":
                        return SyncStrategy.Passive;
                    default:
                        return SyncStrategy.None;
                }
            }
            set
            {
                switch (value)
                {
                    case SyncStrategy.Active:
                        Set("1", OfflineDatabase.SyncStratPath, Short);
                        break;
                    case SyncStrategy.Passive:
                        Set("2", OfflineDatabase.SyncStratPath, Short);
                        break;
                    default:
                        Set("0", OfflineDatabase.SyncStratPath, Short);
                        break;
                }
            }
        }

        #endregion

        #region Initializers

        public DataNode(RestfulFirebaseApp app, string path)
        {
            App = app;
            Path = path;
        }

        #endregion

        protected string GetUniqueShort()
        {
            string uid = null;
            while (uid == null)
            {
                uid = Helpers.GenerateUID(5, Helpers.Base64Charset);
                var path = Get(OfflineDatabase.LongPath, uid);
                if (path != null) uid = null;
            }
            return uid;
        }

        protected string Get(params string[] path)
        {
            if (path.Any(i => i is null)) return null;
            return App.LocalDatabase.Get(Helpers.CombineUrl(path));
        }

        protected void Set(string data, params string[] path)
        {
            var combined = Helpers.CombineUrl(path);
            if (data == null) App.LocalDatabase.Delete(combined);
            else App.LocalDatabase.Set(combined, data);
        }

        public virtual bool Delete()
        {
            if (!Exist) return false;
            var shortPath = Short;
            var oldBlob = Blob;
            Set(null, OfflineDatabase.ShortPath, Path);
            Set(null, OfflineDatabase.LongPath, shortPath);
            Set(null, OfflineDatabase.SyncBlobPath, shortPath);
            Set(null, OfflineDatabase.ChangesPath, shortPath);
            Set(null, OfflineDatabase.SyncStratPath, shortPath);
            if (oldBlob != Blob) HasBlobChanges = true;
            return true;
        }
    }
}
