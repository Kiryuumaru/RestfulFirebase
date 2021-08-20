using ObservableHelpers;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        public void Set(string path, string data)
        {
            Set(path, data, false);
        }

        /// <summary>
        /// Gets the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to get.
        /// </param>
        /// <returns>
        /// The data of the specified <paramref name="path"/>.
        /// </returns>
        public string Get(string path)
        {
            return Get(path, false);
        }

        /// <summary>
        /// Deletes the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to delete.
        /// </param>
        public void Delete(string path)
        {
            Delete(path, false);
        }

        /// <summary>
        /// Gets the sub paths of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path to get the sub paths.
        /// </param>
        /// <returns>
        /// The sub paths of the specified <paramref name="path"/>.
        /// </returns>
        public IEnumerable<string> GetSubPaths(string path)
        {
            return GetSubPaths(path, false);
        }

        /// <summary>
        /// Check if the specified <paramref name="path"/> exists.
        /// </summary>
        /// <param name="path">
        /// The path to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="path"/> exists; otherwise <c>false</c>.
        /// </returns>
        public bool ContainsData(string path)
        {
            return ContainsData(path, false);
        }

        internal void Set(string path, string data, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            var separated = Utils.UrlSeparate(path);
            var keyHeir = KeyHeirPath;
            for (int i = 0; i < separated.Length - 1; i++)
            {
                keyHeir = Utils.UrlCombine(keyHeir, separated[i]);
                var hiers = DBGet(ValidatePath(keyHeir), tryFromAuthStore);
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                var serialized = Utils.SerializeString(deserialized.ToArray());
                DBSet(ValidatePath(keyHeir), serialized, tryFromAuthStore);
            }
            DBSet(path, data, tryFromAuthStore);
        }

        internal string Get(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            return DBGet(path, tryFromAuthStore);
        }

        internal void Delete(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            var subKeyHierPath = ValidatePath(Utils.UrlCombine(KeyHeirPath, path));
            if (DBGet(subKeyHierPath, tryFromAuthStore) == null)
            {
                var separated = Utils.UrlSeparate(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHierList = separated.Take(i + 1).ToList();
                    if (separated.Length - 1 != i)
                    {
                        var valuePath = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                        if (DBGet(valuePath, tryFromAuthStore) != null) break;
                    }
                    keyHierList = keyHierList.Take(i).ToList();
                    keyHierList.Insert(0, KeyHeirPath);
                    var keyHeir = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                    var hiers = DBGet(keyHeir, tryFromAuthStore);
                    var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                    if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                    if (deserialized.Count == 0)
                    {
                        DBDelete(keyHeir, tryFromAuthStore);
                    }
                    else
                    {
                        var serialized = Utils.SerializeString(deserialized.ToArray());
                        DBSet(keyHeir, serialized, tryFromAuthStore);
                        break;
                    }
                }
            }
            DBDelete(path, tryFromAuthStore);
        }

        internal IEnumerable<string> GetSubPaths(string path, bool tryFromAuthStore)
        {
            List<string> subPaths = new List<string>();
            void recursive(string subPath)
            {
                var s = Utils.UrlCombine(KeyHeirPath, subPath);
                var hiers = DBGet(ValidatePath(s), tryFromAuthStore);
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (path != subPath)
                {
                    if (DBContainsKey(ValidatePath(subPath), tryFromAuthStore))
                    {
                        subPaths.Add(subPath);
                    }
                }
                if (deserialized.Count != 0)
                {
                    foreach (var hier in deserialized)
                    {
                        recursive(Utils.UrlCombine(subPath, hier));
                    }
                }
            }
            recursive(path);
            return subPaths;
        }

        internal bool ContainsData(string path, bool tryFromAuthStore)
        {
            path = ValidatePath(path);
            return DBContainsKey(path, tryFromAuthStore);
        }

        private string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is null or empty");

            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }

        private void DBSet(string key, string value, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = App.Config.LocalEncryption.EncryptValue(value);

            lock (store)
            {
                store.Set(encryptedKey, encryptedValue);
            }
        }

        private string DBGet(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = null;

            lock (store)
            {
                encryptedValue = store.Get(encryptedKey);
            }

            return App.Config.LocalEncryption.DecryptValue(encryptedValue);
        }

        private bool DBContainsKey(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            lock (store)
            {
                return store.ContainsKey(encryptedKey);
            }
        }

        private void DBDelete(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            lock (store)
            {
                store.Delete(encryptedKey);
            }
        }

        private void DBClear(bool tryFromAuthStore)
        {
            var store = DBGetStore(tryFromAuthStore);

            lock (store)
            {
                store.Clear();
            }
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
