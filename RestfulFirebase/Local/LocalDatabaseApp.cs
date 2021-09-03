using ObservableHelpers;
using RestfulFirebase.Utilities;
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
        #region HelperClasses

        private class LockHolder
        {
            public int Lockers { get; set; } = 0;
        }

        #endregion

        #region Properties

        private const string KeyHeirPath = "key";
        private const string ValuePath = "val";

        private ConcurrentDictionary<string, LockHolder> pathLocks = new ConcurrentDictionary<string, LockHolder>();

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
            var separated = UrlUtilities.Separate(path);
            var keyHeir = KeyHeirPath;
            for (int i = 0; i < separated.Length - 1; i++)
            {
                keyHeir = UrlUtilities.Combine(keyHeir, separated[i]);
                var validatedKeyHeir = ValidatePath(keyHeir);
                PathWriteLock(validatedKeyHeir, delegate
                {
                    var hiers = DBGet(validatedKeyHeir, tryFromAuthStore);
                    var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
                    if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                    var serialized = StringUtilities.Serialize(deserialized.ToArray());
                    DBSet(validatedKeyHeir, serialized, tryFromAuthStore);
                });
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
            var subKeyHierPath = ValidatePath(UrlUtilities.Combine(KeyHeirPath, path));
            if (DBGet(subKeyHierPath, tryFromAuthStore) == null)
            {
                var separated = UrlUtilities.Separate(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHierList = separated.Take(i + 1).ToList();
                    if (separated.Length - 1 != i)
                    {
                        var valuePath = ValidatePath(UrlUtilities.Combine(keyHierList.ToArray()));
                        if (DBGet(valuePath, tryFromAuthStore) != null)
                        {
                            break;
                        }
                    }
                    keyHierList = keyHierList.Take(i).ToList();
                    keyHierList.Insert(0, KeyHeirPath);
                    var keyHeir = ValidatePath(UrlUtilities.Combine(keyHierList.ToArray()));
                    bool isBreak = false;
                    PathWriteLock(keyHeir, delegate
                    {
                        var hiers = DBGet(keyHeir, tryFromAuthStore);
                        var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
                        if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                        if (deserialized.Count == 0)
                        {
                            DBDelete(keyHeir, tryFromAuthStore);
                        }
                        else
                        {
                            var serialized = StringUtilities.Serialize(deserialized.ToArray());
                            DBSet(keyHeir, serialized, tryFromAuthStore);
                            isBreak = true;
                        }
                    });
                    if (isBreak)
                    {
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
                var keyHeir = ValidatePath(UrlUtilities.Combine(KeyHeirPath, subPath));
                var hiers = DBGet(keyHeir, tryFromAuthStore);
                var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
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
                        recursive(UrlUtilities.Combine(subPath, hier));
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

        private void PathWriteLock(string path, Action action)
        {
            LockHolder pathLock = pathLocks.GetOrAdd(path, delegate { return new LockHolder(); });
            pathLock.Lockers++;
            lock (pathLock)
            {
                action?.Invoke();
                pathLock.Lockers--;
                if (pathLock.Lockers == 0)
                {
                    pathLocks.TryRemove(path, out _);
                }
            }
        }

        private void DBSet(string key, string value, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = App.Config.LocalEncryption.EncryptValue(value);

            store.Set(encryptedKey, encryptedValue);
        }

        private string DBGet(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = null;

            encryptedValue = store.Get(encryptedKey);

            return App.Config.LocalEncryption.DecryptValue(encryptedValue);
        }

        private bool DBContainsKey(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            return store.ContainsKey(encryptedKey);
        }

        private void DBDelete(string key, bool tryFromAuthStore)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            var store = DBGetStore(tryFromAuthStore);

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            store.Delete(encryptedKey);
        }

        private void DBClear(bool tryFromAuthStore)
        {
            var store = DBGetStore(tryFromAuthStore);

            store.Clear();
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
