using Newtonsoft.Json;
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
    internal class DataHolder
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        public string Uri { get; }

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
        }

        #endregion

        #region Methods

        public async Task<bool> IsExists()
        {
            return await GetShort().ConfigureAwait(false) != null;
        }

        public async Task<string> GetShort()
        {
            if (!isShortKeyLoaded)
            {
                isShortKeyLoaded = true;
                shortKeyCache = await Get(OfflineDatabase.ShortPath, Uri).ConfigureAwait(false);
            }
            return shortKeyCache;
        }

        public async Task<string> GetSync()
        {
            if (!isSyncLoaded)
            {
                isSyncLoaded = true;
                var shortKey = await GetShort();
                syncCache = shortKey == null ? null : await Get(OfflineDatabase.SyncBlobPath, shortKey).ConfigureAwait(false);
            }
            return syncCache;
        }

        public async Task<DataChanges> GetChanges()
        {
            if (!isChangesLoaded)
            {
                isChangesLoaded = true;
                var shortKey = await GetShort();
                changesCache = shortKey == null ? null : DataChanges.Parse(await Get(OfflineDatabase.ChangesPath, shortKey).ConfigureAwait(false));
            }
            return changesCache;
        }

        public async Task<string> GetBlob()
        {
            if (!isBlobLoaded)
            {
                isBlobLoaded = true;
                var changes = await GetChanges().ConfigureAwait(false);
                var sync = await GetSync().ConfigureAwait(false);
                blobCache = changes == null ? sync : changes.Blob;
            }
            return blobCache;
        }

        public IEnumerable<string> GetHierarchyUri()
        {
            if (!isHierarchyUriLoaded)
            {
                isHierarchyUriLoaded = true;
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
            }
            return hierarchyUriCache;
        }

        internal async Task<bool> MakeChanges(string blob, Action<RetryExceptionEventArgs> error)
        {
            App.Database.OfflineDatabase.EvaluateCache(this);

            var oldBlob = await GetBlob().ConfigureAwait(false);

            if (await GetSync().ConfigureAwait(false) == null)
            {
                if (blob == null)
                {
                    await SetChanges(null).ConfigureAwait(false);
                    //Put(onError);
                }
                else
                {
                    await SetChanges(new DataChanges(
                        blob,
                        DataChangesType.Create)).ConfigureAwait(false);
                    await Put(error).ConfigureAwait(false);
                }
            }
            else if (oldBlob != blob)
            {
                await SetChanges(new DataChanges(
                    blob,
                    blob == null ? DataChangesType.Delete : DataChangesType.Update)).ConfigureAwait(false);
                await Put(error).ConfigureAwait(false);
            }
            else
            {
                await Put(error).ConfigureAwait(false);
            }

            return oldBlob != await GetBlob().ConfigureAwait(false);
        }

        internal async Task<bool> MakeSync(string sync, Action<RetryExceptionEventArgs> error)
        {
            App.Database.OfflineDatabase.EvaluateCache(this);

            var oldBlob = await GetBlob().ConfigureAwait(false);
            var currentSync = await GetSync().ConfigureAwait(false);
            var currentChanges = await GetChanges().ConfigureAwait(false);

            if (currentChanges?.Blob == null)
            {
                if (sync == null)
                {
                    await Delete().ConfigureAwait(false);
                }
                else
                {
                    await SetSync(sync).ConfigureAwait(false);
                }
            }
            else if (currentChanges.Blob == sync)
            {
                await SetSync(sync).ConfigureAwait(false);
                await DeleteChanges().ConfigureAwait(false);
            }
            else
            {
                switch (currentChanges.ChangesType)
                {
                    case DataChangesType.Create:
                        if (sync == null)
                        {
                            await Put(error).ConfigureAwait(false);
                        }
                        else
                        {
                            await SetSync(sync).ConfigureAwait(false);
                            await DeleteChanges().ConfigureAwait(false);
                        }
                        break;
                    case DataChangesType.Update:
                        if (sync == null)
                        {
                            await Delete().ConfigureAwait(false);
                        }
                        else if (currentSync == sync)
                        {
                            await Put(error).ConfigureAwait(false);
                        }
                        else
                        {
                            await SetSync(sync).ConfigureAwait(false);
                            await DeleteChanges().ConfigureAwait(false);
                        }
                        break;
                    case DataChangesType.Delete:
                        if (sync == null)
                        {
                            break;
                        }
                        if (currentSync == sync)
                        {
                            await Put(error).ConfigureAwait(false);
                        }
                        else
                        {
                            await SetSync(sync).ConfigureAwait(false);
                            await DeleteChanges().ConfigureAwait(false);
                        }
                        break;
                    case DataChangesType.None:
                        await SetSync(sync).ConfigureAwait(false);
                        await DeleteChanges().ConfigureAwait(false);
                        break;
                }
            }

            return oldBlob != sync;
        }

        internal async Task<bool> DeleteChanges()
        {
            App.Database.OfflineDatabase.CancelPut(Uri);
            var hasChanges = await GetChanges().ConfigureAwait(false) != null;
            await SetChanges(null).ConfigureAwait(false);
            return hasChanges;
        }

        internal async Task<bool> Delete()
        {
            if (!await IsExists())
            {
                return false;
            }
            App.Database.OfflineDatabase.CancelPut(Uri);
            var shortPath = await GetShort().ConfigureAwait(false);
            await Set(null, OfflineDatabase.ShortPath, Uri).ConfigureAwait(false);
            await Set(null, OfflineDatabase.SyncBlobPath, shortPath).ConfigureAwait(false);
            await Set(null, OfflineDatabase.ChangesPath, shortPath).ConfigureAwait(false);
            return true;
        }
        private async Task SetShort(string shortKey)
        {
            await Set(shortKey, OfflineDatabase.ShortPath, Uri).ConfigureAwait(false);
            shortKeyCache = shortKey;
            isShortKeyLoaded = true;

            isSyncLoaded = false;
            isChangesLoaded = false;
            isBlobLoaded = false;
        }

        private async Task SetSync(string sync)
        {
            var shortKey = await GetShort();
            if (shortKey == null)
            {
                if (sync == null)
                {
                    return;
                }
                await SetShort(await GetUniqueShort().ConfigureAwait(false)).ConfigureAwait(false);
            }
            await Set(sync, OfflineDatabase.SyncBlobPath, shortKey).ConfigureAwait(false);
            syncCache = sync;
            isSyncLoaded = true;

            isBlobLoaded = false;
        }

        private async Task SetChanges(DataChanges changes)
        {
            var currentShortKey = await GetShort();
            if (currentShortKey == null)
            {
                if (changes == null)
                {
                    return;
                }
                await SetShort(await GetUniqueShort().ConfigureAwait(false)).ConfigureAwait(false);
            }
            await Set(changes?.ToData(), OfflineDatabase.ChangesPath, currentShortKey).ConfigureAwait(false);
            changesCache = changes;
            isChangesLoaded = true;

            isBlobLoaded = false;
        }

        private async Task Put(Action<RetryExceptionEventArgs> error)
        {
            await App.Database.OfflineDatabase.Put(this, error).ConfigureAwait(false);
        }

        private async Task<string> GetUniqueShort()
        {
            string uid = null;
            while (uid == null)
            {
                uid = UIDFactory.GenerateUID(5, Utils.Base64Charset);
                var sync = await Get(OfflineDatabase.SyncBlobPath, uid).ConfigureAwait(false);
                var changes = await Get(OfflineDatabase.ChangesPath, uid).ConfigureAwait(false);
                if (sync != null || changes != null) uid = null;
            }
            return uid;
        }

        private async Task<string> Get(params string[] path)
        {
            if (path.Any(i => i is null)) return null;
            return await App.LocalDatabase.Get(Utils.UrlCombine(path)).ConfigureAwait(false);
        }

        private async Task Set(string data, params string[] path)
        {
            var combined = Utils.UrlCombine(path);
            if (data == null)
            {
                await App.LocalDatabase.Delete(combined).ConfigureAwait(false);
            }
            else
            {
                await App.LocalDatabase.Set(combined, data).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
