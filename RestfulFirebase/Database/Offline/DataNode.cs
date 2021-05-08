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

        public string Key => Utils.SeparateUrl(Path).Last();

        public bool Exist => Short != null;

        public bool HasBlobChanges { get; private set; }

        public string Short
        {
            get => Get(OfflineDatabase.ShortPath, Path);
            protected set => Set(value, OfflineDatabase.ShortPath, Path);
        }

        public string Sync
        {
            get => Exist ? Get(OfflineDatabase.SyncBlobPath, Short) : null;
            set
            {
                if (Short == null) Short = GetUniqueShort();
                var oldBlob = Blob;
                Set(value, OfflineDatabase.SyncBlobPath, Short);
                if (oldBlob != Blob) HasBlobChanges = true;
            }
        }

        public DataChanges Changes
        {
            get => Exist ? DataChanges.Parse(Get(OfflineDatabase.ChangesPath, Short)) : null;
            set
            {
                if (Short == null) Short = GetUniqueShort();
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
                uid = UIDFactory.GenerateUID(5, Utils.Base64Charset);
                var sync = Get(OfflineDatabase.SyncBlobPath, uid);
                var changes = Get(OfflineDatabase.ChangesPath, uid);
                if (sync != null || changes != null) uid = null;
            }
            return uid;
        }

        protected string Get(params string[] path)
        {
            if (path.Any(i => i is null)) return null;
            return App.LocalDatabase.Get(Utils.CombineUrl(path));
        }

        protected void Set(string data, params string[] path)
        {
            var combined = Utils.CombineUrl(path);
            if (data == null) App.LocalDatabase.Delete(combined);
            else App.LocalDatabase.Set(combined, data);
        }

        public virtual bool Delete()
        {
            if (!Exist) return false;
            var shortPath = Short;
            var oldBlob = Blob;
            Set(null, OfflineDatabase.ShortPath, Path);
            Set(null, OfflineDatabase.SyncBlobPath, shortPath);
            Set(null, OfflineDatabase.ChangesPath, shortPath);
            if (oldBlob != Blob) HasBlobChanges = true;
            return true;
        }
    }
}
