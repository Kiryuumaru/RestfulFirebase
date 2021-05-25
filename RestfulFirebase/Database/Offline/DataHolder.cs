using Newtonsoft.Json;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    internal class DataHolder
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string Uri { get; }

        public bool Exist => Short != null;

        public string Short
        {
            get => Get(OfflineDatabase.ShortPath, Uri);
            internal set => Set(value, OfflineDatabase.ShortPath, Uri);
        }

        public string Sync
        {
            get => Exist ? Get(OfflineDatabase.SyncBlobPath, Short) : null;
            internal set
            {
                if (Short == null) Short = GetUniqueShort();
                Set(value, OfflineDatabase.SyncBlobPath, Short);
            }
        }

        public DataChanges Changes
        {
            get => Exist ? DataChanges.Parse(Get(OfflineDatabase.ChangesPath, Short)) : null;
            internal set
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

        public IEnumerable<string> HierarchyUri
        {
            get
            {
                return App.Database.OfflineDatabase.GetHierUris(Uri);
            }
        }

        #endregion

        #region Initializers

        public DataHolder(RestfulFirebaseApp app, string Uri)
        {
            App = app;
            this.Uri = Uri.EndsWith("/") ? Uri : Uri + "/";
        }

        #endregion

        private void Put(Action<RetryExceptionEventArgs> onError)
        {
            App.Database.OfflineDatabase.Put(this, onError);
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
            return App.LocalDatabase.Get(Utils.UrlCombine(path));
        }

        protected void Set(string data, params string[] path)
        {
            var combined = Utils.UrlCombine(path);
            if (data == null) App.LocalDatabase.Delete(combined);
            else App.LocalDatabase.Set(combined, data);
        }

        internal bool MakeChanges(string blob, Action<RetryExceptionEventArgs> onError)
        {
            var oldBlob = Blob;

            if (Sync == null)
            {
                Changes = new DataChanges(
                    blob,
                    blob == null ? DataChangesType.None : DataChangesType.Create);

                Put(onError);
            }
            else if (Changes == null || oldBlob != blob)
            {
                Changes = new DataChanges(
                    blob,
                    blob == null ? DataChangesType.Delete : DataChangesType.Update);

                Put(onError);
            }

            return oldBlob != Blob;
        }

        internal bool MakeSync(string blob, Action<RetryExceptionEventArgs> onError)
        {
            var oldBlob = Blob;

            if (Changes?.Blob == null)
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
                            Put(onError);
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
                            Put(onError);
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
                            Put(onError);
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

        internal bool DeleteChanges()
        {
            App.Database.OfflineDatabase.CancelPut(this);
            var hasChanges = Changes != null;
            Changes = null;
            return hasChanges;
        }

        internal virtual bool Delete()
        {
            if (!Exist) return false;
            App.Database.OfflineDatabase.CancelPut(this);
            var shortPath = Short;
            Set(null, OfflineDatabase.ShortPath, Uri);
            Set(null, OfflineDatabase.SyncBlobPath, shortPath);
            Set(null, OfflineDatabase.ChangesPath, shortPath);
            return true;
        }
    }
}
