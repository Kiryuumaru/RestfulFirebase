using Newtonsoft.Json;
using ObservableHelpers;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    internal class DataHolder : Disposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string Uri { get; }

        public bool IsExists
        {
            get
            {
                return ShortKey != null;
            }
        }

        public string ShortKey
        {
            get
            {
                if (!isShortKeyLoaded)
                {
                    shortKeyCache = Get(OfflineDatabase.ShortPath, Uri);

                    isShortKeyLoaded = true;
                    isSyncLoaded = false;
                    isChangesLoaded = false;
                    isBlobLoaded = false;
                }
                return shortKeyCache;
            }
            private set
            {
                Set(value, OfflineDatabase.ShortPath, Uri);
                shortKeyCache = value;

                isShortKeyLoaded = true;
                isSyncLoaded = false;
                isChangesLoaded = false;
                isBlobLoaded = false;
            }
        }

        public string Sync
        {
            get
            {
                if (!isSyncLoaded)
                {
                    var shortKey = ShortKey;
                    syncCache = shortKey == null ? null : Get(OfflineDatabase.SyncBlobPath, shortKey);

                    isSyncLoaded = true;
                    isBlobLoaded = false;
                }
                return syncCache;
            }
            private set
            {
                var shortKey = ShortKey;
                if (shortKey == null)
                {
                    if (value == null)
                    {
                        return;
                    }
                    ShortKey = GetUniqueShort();
                }
                Set(value, OfflineDatabase.SyncBlobPath, shortKey);
                syncCache = value;

                isSyncLoaded = true;
                isBlobLoaded = false;
            }
        }

        public DataChanges Changes
        {
            get
            {
                if (!isChangesLoaded)
                {
                    var shortKey = ShortKey;
                    changesCache = shortKey == null ? null : DataChanges.Parse(Get(OfflineDatabase.ChangesPath, shortKey));

                    isChangesLoaded = true;
                    isBlobLoaded = false;
                }
                return changesCache;
            }
            private set
            {
                var shortKey = ShortKey;
                if (shortKey == null)
                {
                    if (value == null)
                    {
                        return;
                    }
                    ShortKey = GetUniqueShort();
                }
                Set(value?.ToData(), OfflineDatabase.ChangesPath, shortKey);
                changesCache = value;

                isChangesLoaded = true;
                isBlobLoaded = false;
            }
        }

        public string Blob
        {
            get
            {
                if (!isBlobLoaded)
                {
                    var changes = Changes;
                    var sync = Sync;
                    blobCache = changes == null ? sync : changes.Blob;

                    isBlobLoaded = true;
                }
                return blobCache;
            }
        }

        public IEnumerable<string> HierarchyUri
        {
            get
            {
                if (!isHierarchyUriLoaded)
                {
                    var hier = new List<string>();
                    var path = Uri.Replace(App.Config.DatabaseURL, "");
                    var separated = Utils.UrlSeparate(path);
                    var currentUri = App.Config.DatabaseURL;
                    hier.Add(currentUri);
                    for (int i = 0; i < separated.Length - 1; i++)
                    {
                        currentUri = Utils.UrlCombine(currentUri, separated[i]);
                        hier.Add(currentUri);
                    }
                    hierarchyUriCache = hier;

                    isHierarchyUriLoaded = true;
                }
                return hierarchyUriCache;
            }
        }

        private bool isShortKeyLoaded;
        private string shortKeyCache;

        private bool isSyncLoaded;
        private string syncCache;

        private bool isChangesLoaded;
        private DataChanges changesCache;

        private bool isBlobLoaded;
        private string blobCache;

        private bool isHierarchyUriLoaded;
        private IEnumerable<string> hierarchyUriCache;

        #endregion

        #region Initializers

        public DataHolder(RestfulFirebaseApp app, string uri)
        {
            App = app;
            Uri = uri.EndsWith("/") ? uri : uri + "/";

            App.Config.PropertyChanged += Config_PropertyChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                App.Config.PropertyChanged -= Config_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        internal bool MakeChanges(string blob, Action<RetryExceptionEventArgs> error)
        {
            App.Database.OfflineDatabase.EvaluateCache(this);

            var oldBlob = Blob;

            if (Sync == null)
            {
                if (blob == null)
                {
                    Changes = null;
                    //Put(onError);
                }
                else
                {
                    Changes = new DataChanges(
                        blob,
                        DataChangesType.Create);
                    Put(error);
                }
            }
            else if (oldBlob != blob)
            {
                Changes = new DataChanges(
                    blob,
                    blob == null ? DataChangesType.Delete : DataChangesType.Update);
                Put(error);
            }
            else if (blob == null)
            {
                Put(error);
            }

            return oldBlob != Blob;
        }

        internal bool MakeSync(string sync, Action<RetryExceptionEventArgs> error)
        {
            App.Database.OfflineDatabase.EvaluateCache(this);

            var oldBlob = Blob;
            var currentSync = Sync;
            var currentChanges = Changes;

            if (currentChanges?.Blob == null)
            {
                if (sync == null)
                {
                    Delete();
                }
                else
                {
                    Sync = sync;
                }
            }
            else if (currentChanges.Blob == sync)
            {
                Sync = sync;
                DeleteChanges();
            }
            else
            {
                switch (currentChanges.ChangesType)
                {
                    case DataChangesType.Create:
                        if (sync == null)
                        {
                            Put(error);
                        }
                        else
                        {
                            Sync = sync;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.Update:
                        if (sync == null)
                        {
                            Delete();
                        }
                        else if (currentSync == sync)
                        {
                            Put(error);
                        }
                        else
                        {
                            Sync = sync;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.Delete:
                        if (sync == null)
                        {
                            break;
                        }
                        if (currentSync == sync)
                        {
                            Put(error);
                        }
                        else
                        {
                            Sync = sync;
                            DeleteChanges();
                        }
                        break;
                    case DataChangesType.None:
                        Sync = sync;
                        DeleteChanges();
                        break;
                }
            }

            return oldBlob != sync;
        }

        internal bool DeleteChanges()
        {
            App.Database.OfflineDatabase.CancelPut(Uri);
            var hasChanges = Changes != null;
            Changes = null;
            return hasChanges;
        }

        internal bool Delete()
        {
            if (!IsExists)
            {
                return false;
            }
            App.Database.OfflineDatabase.CancelPut(Uri);
            var shortKey = ShortKey;
            Set(null, OfflineDatabase.ShortPath, Uri);
            Set(null, OfflineDatabase.SyncBlobPath, shortKey);
            Set(null, OfflineDatabase.ChangesPath, shortKey);
            ResetCache();
            return true;
        }

        private void Put(Action<RetryExceptionEventArgs> error)
        {
            App.Database.OfflineDatabase.Put(this, error);
        }

        private string GetUniqueShort()
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

        private string Get(params string[] path)
        {
            if (path.Any(i => i is null)) return null;
            return App.LocalDatabase.Get(Utils.UrlCombine(path));
        }

        private void Set(string data, params string[] path)
        {
            var combined = Utils.UrlCombine(path);
            if (data == null)
            {
                App.LocalDatabase.Delete(combined);
            }
            else
            {
                App.LocalDatabase.Set(combined, data);
            }
        }

        private void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.Config.LocalDatabase) ||
                e.PropertyName == nameof(App.Config.CustomAuthLocalDatabase))
            {
                ResetCache();
            }
        }

        private void ResetCache()
        {
            isShortKeyLoaded = false;
            isSyncLoaded = false;
            isChangesLoaded = false;
            isBlobLoaded = false;
        }

        #endregion
    }
}
