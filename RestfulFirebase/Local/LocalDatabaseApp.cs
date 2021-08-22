using ObservableHelpers;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Local
{
    /// <summary>
    /// App module that provides persistency for the <see cref="RestfulFirebaseApp"/>.
    /// </summary>
    public class LocalDatabaseApp : SyncContext, IAppModule
    {
        #region Properties

        private const string EndNodeClassifier = "0";
        private const string BranchNodeClassifier = "1";
        private const string KeyHeirPath = "key";
        private const string ValuePath = "val";

        private ConcurrentDictionary<string, string> cacheDb = new ConcurrentDictionary<string, string>();

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; }

        private SemaphoreSlim storeLock = new SemaphoreSlim(1, 1);

        #endregion

        #region Initializers

        internal LocalDatabaseApp(RestfulFirebaseApp app)
        {
            SyncOperation.SetContext(app);

            App = app;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to set.
        /// </param>
        /// <param name="data">
        /// The data to set.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the completion of data set.
        /// </returns>
        public async Task Set(string path, string data)
        {
            await Set(path, data, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to get.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that holds the data of the specified <paramref name="path"/>.
        /// </returns>
        public async Task<string> Get(string path)
        {
            return await Get(path, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to delete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the completion of data deletion.
        /// </returns>
        public async Task Delete(string path)
        {
            await Delete(path, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the sub paths of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path to get the sub paths.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that holds the sub paths of the specified <paramref name="path"/>.
        /// </returns>
        public async Task<IEnumerable<string>> GetSubPaths(string path)
        {
            return await GetSubPaths(path, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Check if the specified <paramref name="path"/> exists.
        /// </summary>
        /// <param name="path">
        /// The path to check.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that holds <c>true</c> if the <paramref name="path"/> exists; otherwise <c>false</c>.
        /// </returns>
        public async Task<bool> ContainsData(string path)
        {
            return await ContainsData(path, false).ConfigureAwait(false);
        }

        internal async Task Set(string path, string data, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            var separated = Utils.UrlSeparate(path);
            var keyHeir = KeyHeirPath;
            for (int i = 0; i < separated.Length - 1; i++)
            {
                keyHeir = Utils.UrlCombine(keyHeir, separated[i]);
                var hiers = await DBGet(ValidatePath(keyHeir), tryFromAuthStore).ConfigureAwait(false);
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                var serialized = Utils.SerializeString(deserialized.ToArray());
                await DBSet(ValidatePath(keyHeir), serialized, tryFromAuthStore).ConfigureAwait(false);
            }
            await DBSet(path, data, tryFromAuthStore).ConfigureAwait(false);
        }

        internal async Task<string> Get(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            return await DBGet(path, tryFromAuthStore).ConfigureAwait(false);
        }

        internal async Task Delete(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            var subKeyHierPath = ValidatePath(Utils.UrlCombine(KeyHeirPath, path));
            if (await DBGet(subKeyHierPath, tryFromAuthStore).ConfigureAwait(false) == null)
            {
                var separated = Utils.UrlSeparate(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHierList = separated.Take(i + 1).ToList();
                    if (separated.Length - 1 != i)
                    {
                        var valuePath = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                        if (await DBGet(valuePath, tryFromAuthStore).ConfigureAwait(false) != null)
                        {
                            break;
                        }
                    }
                    keyHierList = keyHierList.Take(i).ToList();
                    keyHierList.Insert(0, KeyHeirPath);
                    var keyHeir = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                    var hiers = await DBGet(keyHeir, tryFromAuthStore).ConfigureAwait(false);
                    var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                    if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                    if (deserialized.Count == 0)
                    {
                        await DBDelete(keyHeir, tryFromAuthStore).ConfigureAwait(false);
                    }
                    else
                    {
                        var serialized = Utils.SerializeString(deserialized.ToArray());
                        await DBSet(keyHeir, serialized, tryFromAuthStore).ConfigureAwait(false);
                        break;
                    }
                }
            }
            await DBDelete(path, tryFromAuthStore).ConfigureAwait(false);
        }

        internal async Task<IEnumerable<string>> GetSubPaths(string path, bool tryFromAuthStore)
        {
            List<string> subPaths = new List<string>();
            async Task recursive(string subPath)
            {
                var s = Utils.UrlCombine(KeyHeirPath, subPath);
                var hiers = await DBGet(ValidatePath(s), tryFromAuthStore).ConfigureAwait(false);
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (path != subPath)
                {
                    if (await DBContainsKey(ValidatePath(subPath), tryFromAuthStore).ConfigureAwait(false))
                    {
                        subPaths.Add(subPath);
                    }
                }
                if (deserialized.Count != 0)
                {
                    foreach (var hier in deserialized)
                    {
                        await recursive(Utils.UrlCombine(subPath, hier)).ConfigureAwait(false);
                    }
                }
            }
            await recursive(path).ConfigureAwait(false);
            return subPaths;
        }

        internal async Task<bool> ContainsData(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            return await DBContainsKey(path, tryFromAuthStore).ConfigureAwait(false);
        }

        private string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is null or empty");

            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }

        private async Task DBSet(string key, string value, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = App.Config.LocalEncryption.EncryptValue(value);

            await storeLock.WaitAsync().ConfigureAwait(false);

            store.Set(encryptedKey, encryptedValue);

            storeLock.Release();
        }

        private async Task<string> DBGet(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = null;

            await storeLock.WaitAsync().ConfigureAwait(false);

            encryptedValue = store.Get(encryptedKey);

            storeLock.Release();

            return App.Config.LocalEncryption.DecryptValue(encryptedValue);
        }

        private async Task<bool> DBContainsKey(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            await storeLock.WaitAsync().ConfigureAwait(false);

            bool result = store.ContainsKey(encryptedKey);

            storeLock.Release();

            return result;
        }

        private async Task DBDelete(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            await storeLock.WaitAsync().ConfigureAwait(false);

            store.Delete(encryptedKey);

            storeLock.Release();
        }

        private async Task DBClear(bool tryFromAuthStore)
        {
            var store = DBGetStore(tryFromAuthStore);

            await storeLock.WaitAsync().ConfigureAwait(false);

            store.Clear();

            storeLock.Release();
        }

        private ILocalDatabase DBGetStore(bool tryFromAuthStore)
        {
            return tryFromAuthStore
                ? (App.Config.CustomAuthLocalDatabase ?? App.Config.LocalDatabase)
                : App.Config.LocalDatabase;
        }

        #endregion
    }
}
