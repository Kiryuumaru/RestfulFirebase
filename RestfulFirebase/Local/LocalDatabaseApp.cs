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
            Set(App.Config.LocalDatabase, path, data);
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
            return Get(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Deletes the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to delete.
        /// </param>
        public void Delete(string path)
        {
            Delete(App.Config.LocalDatabase, path);
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
            return GetSubPaths(App.Config.LocalDatabase, path);
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
            return ContainsData(App.Config.LocalDatabase, path);
        }

        internal void Set(ILocalDatabase localDatabase, string path, string data)
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
                    var hiers = DBGet(localDatabase, validatedKeyHeir);
                    var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
                    if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                    var serialized = StringUtilities.Serialize(deserialized.ToArray());
                    DBSet(localDatabase, validatedKeyHeir, serialized);
                });
            }
            DBSet(localDatabase, path, data);
        }

        internal string Get(ILocalDatabase localDatabase, string path)
        {
            path = ValidatePath(path);
            return DBGet(localDatabase, path);
        }

        internal void Delete(ILocalDatabase localDatabase, string path)
        {
            path = ValidatePath(path);
            var subKeyHierPath = ValidatePath(UrlUtilities.Combine(KeyHeirPath, path));
            if (DBGet(localDatabase, subKeyHierPath) == null)
            {
                var separated = UrlUtilities.Separate(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHierList = separated.Take(i + 1).ToList();
                    if (separated.Length - 1 != i)
                    {
                        var valuePath = ValidatePath(UrlUtilities.Combine(keyHierList.ToArray()));
                        if (DBGet(localDatabase, valuePath) != null)
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
                        var hiers = DBGet(localDatabase, keyHeir);
                        var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
                        if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                        if (deserialized.Count == 0)
                        {
                            DBDelete(localDatabase, keyHeir);
                        }
                        else
                        {
                            var serialized = StringUtilities.Serialize(deserialized.ToArray());
                            DBSet(localDatabase, keyHeir, serialized);
                            isBreak = true;
                        }
                    });
                    if (isBreak)
                    {
                        break;
                    }
                }
            }
            DBDelete(localDatabase, path);
        }

        internal IEnumerable<string> GetSubPaths(ILocalDatabase localDatabase, string path)
        {
            List<string> subPaths = new List<string>();
            void recursive(string subPath)
            {
                var keyHeir = ValidatePath(UrlUtilities.Combine(KeyHeirPath, subPath));
                var hiers = DBGet(localDatabase, keyHeir);
                var deserialized = StringUtilities.Deserialize(hiers)?.ToList() ?? new List<string>();
                if (path != subPath)
                {
                    if (DBContainsKey(localDatabase, ValidatePath(subPath)))
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

        internal bool ContainsData(ILocalDatabase localDatabase, string path)
        {
            path = ValidatePath(path);
            return DBContainsKey(localDatabase, path);
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

        private void DBSet(ILocalDatabase localDatabase, string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = App.Config.LocalEncryption.EncryptValue(value);

            localDatabase.Set(encryptedKey, encryptedValue);
        }

        private string DBGet(ILocalDatabase localDatabase, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = null;

            encryptedValue = localDatabase.Get(encryptedKey);

            return App.Config.LocalEncryption.DecryptValue(encryptedValue);
        }

        private bool DBContainsKey(ILocalDatabase localDatabase, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            return localDatabase.ContainsKey(encryptedKey);
        }

        private void DBDelete(ILocalDatabase localDatabase, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);

            localDatabase.Delete(encryptedKey);
        }

        private void DBClear(ILocalDatabase localDatabase)
        {
            localDatabase.Clear();
        }

        #endregion
    }
}
