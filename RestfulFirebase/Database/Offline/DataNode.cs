using Newtonsoft.Json;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    public class DataNode
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public RealtimeWire Wire { get; }

        public string Path { get; }

        public string Key => Utils.SeparateUrl(Path).Last();

        public bool Exist => Short != null;

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
                Set(value, OfflineDatabase.SyncBlobPath, Short);
            }
        }

        public DataChanges Changes
        {
            get => Exist ? DataChanges.Parse(Get(OfflineDatabase.ChangesPath, Short)) : null;
            private set
            {
                if (Short == null) Short = GetUniqueShort();
                Set(value?.ToData(), OfflineDatabase.ChangesPath, Short);
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
            Wire = RealtimeWire.CreateFromQuery(app, new ChildQuery(app, () => path), false);
        }

        public DataNode(RealtimeWire wire)
        {
            App = wire.App;
            Path = wire.Query.GetAbsolutePath();
            Wire = wire;
        }

        #endregion

        private void Put(string blob, Action<RetryExceptionEventArgs> onError)
        {
            Wire.Put(JsonConvert.SerializeObject(blob), onError);
        }

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

        public bool MakeChanges(string blob, Action<RetryExceptionEventArgs> onError)
        {
            var oldBlob = Blob;

            if (Sync == null)
            {
                Changes = new DataChanges(
                    blob,
                    blob == null ? DataChangesType.None : DataChangesType.Create,
                    App.Database.OfflineDatabase.GetAvailableSyncPriority());

                Put(blob, onError);
            }
            else if (Changes == null || oldBlob != blob)
            {
                Changes = new DataChanges(
                    blob,
                    blob == null ? DataChangesType.Delete : DataChangesType.Update,
                    App.Database.OfflineDatabase.GetAvailableSyncPriority());

                Put(blob, onError);
            }

            return oldBlob != Blob;
        }

        public bool MakeSync(string blob, Action<RetryExceptionEventArgs> onError)
        {
            var oldBlob = Blob;

            if (Changes == null)
            {
                if (blob == null) Delete();
                else Sync = blob;
            }
            else if (Changes.Blob == blob)
            {
                Sync = blob;
                DeleteChanges();
            }
            else
            {
                switch (Changes.ChangesType)
                {
                    case DataChangesType.Create:
                        if (blob == null)
                        {
                            Put(Changes.Blob, onError);
                        }
                        else
                        {
                            Sync = blob;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.Update:
                        if (blob == null)
                        {
                            Delete();
                        }
                        else if (Sync == blob)
                        {
                            Put(Changes.Blob, onError);
                        }
                        else
                        {
                            Sync = blob;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.Delete:
                        if (blob == null)
                        {
                            break;
                        }
                        if (Sync == blob)
                        {
                            Put(null, onError);
                        }
                        else
                        {
                            Sync = blob;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.None:
                        Sync = blob;
                        DeleteChanges();
                        break;
                }
            }

            return oldBlob != Blob;
        }

        public bool DeleteChanges()
        {
            var hasChanges = Changes != null;
            Changes = null;
            return hasChanges;
        }

        public virtual bool Delete()
        {
            if (!Exist) return false;
            var shortPath = Short;
            Set(null, OfflineDatabase.ShortPath, Path);
            Set(null, OfflineDatabase.SyncBlobPath, shortPath);
            Set(null, OfflineDatabase.ChangesPath, shortPath);
            return true;
        }
    }
}
