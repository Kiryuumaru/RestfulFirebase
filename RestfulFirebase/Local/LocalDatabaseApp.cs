// Partial locking mechianism

using ObservableHelpers;
using ObservableHelpers.Utilities;
using RestfulFirebase.Exceptions;
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
        #region Properties

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; }

        private const char ValueIndicator = 'v';
        private const char PathIndicator = 'p';

        private RWLockDictionary<string[]> rwLock = new RWLockDictionary<string[]>(LockRecursionPolicy.SupportsRecursion, PathEqualityComparer.Instance);

        private static RWLock databaseDictionaryLock = new RWLock(LockRecursionPolicy.SupportsRecursion);
        private static Dictionary<ILocalDatabase, LocalDatabaseEventHolder> databaseDictionary = new Dictionary<ILocalDatabase, LocalDatabaseEventHolder>();

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
        /// Subscribe to local database changes.
        /// </summary>
        /// <param name="changesHandler">
        /// The handler to subscribe.
        /// </param>
        public void Subscribe(EventHandler<DataChangesEventArgs> changesHandler)
        {
            InternalSubscribe(App.Config.LocalDatabase, changesHandler);
        }

        /// <summary>
        /// Unsubscribe to local database changes.
        /// </summary>
        /// <param name="changesHandler">
        /// The handler to unsubscribe.
        /// </param>
        public void Unsubscribe(EventHandler<DataChangesEventArgs> changesHandler)
        {
            InternalUnsubscribe(App.Config.LocalDatabase, changesHandler);
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
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public bool Contains(params string[] path)
        {
            return InternalContains(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Deletes the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to delete.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public void Delete(params string[] path)
        {
            InternalDelete(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets the children of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the children to get.
        /// </param>
        /// <returns>
        /// The children of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public (string[] path, string key)[] GetChildren(params string[] path)
        {
            return InternalGetChildren(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets the children relative to the specified <paramref name="path"/> with its corresponding <see cref="LocalDataType"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the children to get.
        /// </param>
        /// <returns>
        /// The children of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public (string key, LocalDataType type)[] GetRelativeTypedChildren(params string[] path)
        {
            return InternalGetRelativeTypedChildren(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets the children with its corresponding <see cref="LocalDataType"/> of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the children to get.
        /// </param>
        /// <returns>
        /// The children of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public (string[] path, LocalDataType type)[] GetTypedChildren(params string[] path)
        {
            return InternalGetTypedChildren(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets the data type of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to get.
        /// </param>
        /// <returns>
        /// The data type of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public LocalDataType GetDataType(params string[] path)
        {
            return InternalGetDataType(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets all the recursive children of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the children to get.
        /// </param>
        /// <returns>
        /// The children of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public string[][] GetRecursiveChildren(params string[] path)
        {
            return InternalGetRecursiveChildren(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets all the recursive children relative to the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the children to get.
        /// </param>
        /// <returns>
        /// The children of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public string[][] GetRecursiveRelativeChildren(params string[] path)
        {
            return InternalGetRecursiveRelativeChildren(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Gets the value of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the value to get.
        /// </param>
        /// <returns>
        /// The value of the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public string GetValue(params string[] path)
        {
            return InternalGetValue(App.Config.LocalDatabase, path);
        }

        /// <summary>
        /// Sets the data of the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to set.
        /// </param>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="StringNullOrEmptyException">
        /// <paramref name="path"/> has null or empty path.
        /// </exception>
        public void SetValue(string value, params string[] path)
        {
            InternalSetValue(App.Config.LocalDatabase, value, path);
        }

        #endregion

        #region Internal Implementations

        internal void InternalSubscribe(ILocalDatabase localDatabase, EventHandler<DataChangesEventArgs> changesHandler)
        {
            databaseDictionaryLock.LockRead(() =>
            {
                if (databaseDictionary.TryGetValue(localDatabase, out var data))
                {
                    data.Changes += changesHandler;
                }
                else
                {
                    databaseDictionaryLock.LockWrite(() => databaseDictionary.Add(localDatabase, new LocalDatabaseEventHolder(this, changesHandler)));
                }
            });
        }

        internal void InternalUnsubscribe(ILocalDatabase localDatabase, EventHandler<DataChangesEventArgs> changesHandler)
        {
            databaseDictionaryLock.LockRead(() =>
            {
                if (databaseDictionary.TryGetValue(localDatabase, out var data))
                {
                    data.Changes -= changesHandler;
                }
                else
                {
                    databaseDictionaryLock.LockWrite(() => databaseDictionary.Add(localDatabase, new LocalDatabaseEventHolder(this, delegate{ })));
                }
            });
        }

        internal bool InternalContains(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);

            return LockReadHierarchy(path, () => DBContains(localDatabase, serializedPath));
        }

        internal void InternalDelete(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);

            LockReadHierarchy(path, () =>
            {
                LocalDatabaseEventHolder holder = GetHandler(localDatabase);

                DeleteChildren(localDatabase, holder, true, path, serializedPath);

                string[] hierPath = path;
                string lastNode = null;

                while (true)
                {
                    if (hierPath.Length > 1)
                    {
                        int newHierLength = hierPath.Length - 1;
                        lastNode = hierPath[newHierLength];
                        hierPath = new string[newHierLength];
                        Array.Copy(path, 0, hierPath, 0, newHierLength);
                    }
                    else
                    {
                        break;
                    }

                    string serializedHierPath = StringUtilities.Serialize(hierPath);
                    string hierData = DBGet(localDatabase, serializedHierPath);
                    if (hierData != null)
                    {
                        if (hierData.Length > 1 && hierData[0] == PathIndicator)
                        {
                            string[] deserialized = StringUtilities.Deserialize(hierData.Substring(1));
                            if (deserialized != null)
                            {
                                int indexOf = Array.IndexOf(deserialized, lastNode);
                                if (indexOf != -1)
                                {
                                    if (deserialized.Length == 1)
                                    {
                                        rwLock.LockWrite(hierPath, () => DBDelete(localDatabase, serializedHierPath));
                                        OnDataChanges(holder, hierPath);
                                    }
                                    else
                                    {
                                        string[] modified = deserialized.RemoveAt(indexOf);
                                        string data = PathIndicator + StringUtilities.Serialize(modified);
                                        rwLock.LockWrite(hierPath, () => DBSet(localDatabase, serializedHierPath, data));
                                        OnDataChanges(holder, hierPath);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            rwLock.LockWrite(hierPath, () => DBDelete(localDatabase, serializedHierPath));
                            OnDataChanges(holder, hierPath);
                        }
                    }
                }
            });
        }

        internal (string[] path, string key)[] InternalGetChildren(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);

            string data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

            if (data?.Length > 0 && data[0] == PathIndicator)
            {
                string[] deserialized = StringUtilities.Deserialize(data.Substring(1));
                if (deserialized != null)
                {
                    (string[] path, string key)[] paths = new (string[] path, string key)[deserialized.Length];
                    for (int i = 0; i < deserialized.Length; i++)
                    {
                        string[] subPath = new string[path.Length + 1];
                        subPath[subPath.Length - 1] = deserialized[i];
                        Array.Copy(path, 0, subPath, 0, path.Length);
                        paths[i] = (subPath, deserialized[i]);
                    }
                    return paths;
                }
            }
            return new (string[] path, string key)[0];
        }

        internal (string[] path, LocalDataType type)[] InternalGetTypedChildren(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            List<(string[] path, LocalDataType type)> paths = new List<(string[] path, LocalDataType type)>();

            void recursive(string[] recvPath, string serializedRecvPath, int root)
            {
                int nextRoot = root + 1;

                string recvData = DBGet(localDatabase, serializedRecvPath);

                if (root == 0)
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(subPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                }
                else
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        paths.Add((recvPath, LocalDataType.Path));
                    }
                    else
                    {
                        paths.Add((recvPath, LocalDataType.Value));
                    }
                }
            }

            string serializedPath = StringUtilities.Serialize(path);
            LockReadHierarchy(path, () => recursive(path, serializedPath, 0));

            return paths.ToArray();
        }

        internal (string key, LocalDataType type)[] InternalGetRelativeTypedChildren(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            List<(string key, LocalDataType type)> paths = new List<(string key, LocalDataType type)>();

            void recursive(string[] recvPath, string serializedRecvPath, int root)
            {
                int nextRoot = root + 1;

                string recvData = DBGet(localDatabase, serializedRecvPath);

                if (root == 0)
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(subPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                }
                else
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        paths.Add((recvPath[recvPath.Length - 1], LocalDataType.Path));
                    }
                    else
                    {
                        paths.Add((recvPath[recvPath.Length - 1], LocalDataType.Value));
                    }
                }
            }

            string serializedPath = StringUtilities.Serialize(path);
            LockReadHierarchy(path, () => recursive(path, serializedPath, 0));

            return paths.ToArray();
        }

        internal LocalDataType InternalGetDataType(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);

            string data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

            if (data?.Length > 0 && data[0] == PathIndicator)
            {
                return LocalDataType.Path;
            }
            else
            {
                return LocalDataType.Value;
            }
        }

        internal string[][] InternalGetRecursiveChildren(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            List<string[]> paths = new List<string[]>();

            void recursive(string[] recvPath, string serializedRecvPath, int root)
            {
                int nextRoot = root + 1;

                string recvData = DBGet(localDatabase, serializedRecvPath);

                if (root == 0)
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                }
                else
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                    else
                    {
                        paths.Add(recvPath);
                    }
                }
            }

            string serializedPath = StringUtilities.Serialize(path);
            LockReadHierarchy(path, () => recursive(path, serializedPath, 0));

            return paths.ToArray();
        }

        internal string[][] InternalGetRecursiveRelativeChildren(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            List<string[]> paths = new List<string[]>();

            void recursive(string[] recvPath, string serializedRecvPath, int root)
            {
                int nextRoot = root + 1;

                string recvData = DBGet(localDatabase, serializedRecvPath);

                if (root == 0)
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                }
                else
                {
                    if (recvData?.Length > 0 && recvData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(recvData.Substring(1));
                        if (deserialized != null)
                        {
                            for (int i = 0; i < deserialized.Length; i++)
                            {
                                string[] subPath = new string[recvPath.Length + 1];
                                subPath[subPath.Length - 1] = deserialized[i];
                                Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                                string serializedSubPath = StringUtilities.Serialize(subPath);
                                rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                            }
                        }
                    }
                    else
                    {
                        string[] pathToAdd = new string[root];
                        Array.Copy(recvPath, recvPath.Length - root, pathToAdd, 0, root);
                        paths.Add(pathToAdd);
                    }
                }
            }

            string serializedPath = StringUtilities.Serialize(path);
            LockReadHierarchy(path, () => recursive(path, serializedPath, 0));

            return paths.ToArray();
        }

        internal string InternalGetValue(ILocalDatabase localDatabase, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);

            string data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

            if (data?.Length > 0)
            {
                return data[0] == ValueIndicator ? data.Substring(1) : default;
            }
            else
            {
                return default;
            }
        }

        internal void InternalSetValue(ILocalDatabase localDatabase, string value, string[] path)
        {
            Validate(localDatabase, path);

            string serializedPath = StringUtilities.Serialize(path);
            LockReadHierarchy(path, () =>
            {
                LocalDatabaseEventHolder holder = GetHandler(localDatabase);

                DeleteChildren(localDatabase, holder, false, path, serializedPath);

                if (path.Length > 1)
                {
                    (string[] path, string serializedPath)[] absolutePaths = new (string[] path, string serializedPath)[path.Length - 1];
                    int startIndex = path.Length - 2;
                    string lastValueToSet = null;

                    for (int i = path.Length - 2; i >= 0; i--)
                    {
                        startIndex = i;
                        int nextI = i + 1;
                        string[] keyHier = new string[nextI];
                        Array.Copy(path, 0, keyHier, 0, nextI);
                        string serializedKeyHier = StringUtilities.Serialize(keyHier);
                        absolutePaths[i] = (keyHier, serializedKeyHier);

                        string data = DBGet(localDatabase, serializedKeyHier);
                        if (data?.Length > 0 && data[0] == PathIndicator)
                        {
                            string[] deserialized = StringUtilities.Deserialize(data.Substring(1));
                            if (deserialized?.Length == 0)
                            {
                                continue;
                            }

                            if (!deserialized.Contains(path[nextI]))
                            {
                                string[] modified = new string[deserialized.Length + 1];
                                Array.Copy(deserialized, 0, modified, 0, deserialized.Length);
                                modified[modified.Length - 1] = path[nextI];
                                lastValueToSet = PathIndicator + StringUtilities.Serialize(modified);
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (lastValueToSet != null)
                    {
                        rwLock.LockWrite(
                            absolutePaths[startIndex].path,
                            () => DBSet(localDatabase, absolutePaths[startIndex].serializedPath, lastValueToSet));
                        OnDataChanges(holder, absolutePaths[startIndex].path);

                        for (int i = startIndex + 1; i < absolutePaths.Length; i++)
                        {
                            string valueToSet = PathIndicator + StringUtilities.Serialize(new string[] { path[i + 1] });
                            rwLock.LockWrite(absolutePaths[i].path,
                                () => DBSet(localDatabase, absolutePaths[i].serializedPath, valueToSet));
                            OnDataChanges(holder, absolutePaths[i].path);
                        }
                    }
                    else
                    {
                        for (int i = startIndex; i < absolutePaths.Length; i++)
                        {
                            string valueToSet = PathIndicator + StringUtilities.Serialize(new string[] { path[i + 1] });
                            rwLock.LockWrite(absolutePaths[i].path,
                                () => DBSet(localDatabase, absolutePaths[i].serializedPath, valueToSet));
                            OnDataChanges(holder, absolutePaths[i].path);
                        }
                    }
                }

                rwLock.LockWrite(path, () => DBSet(localDatabase, serializedPath, ValueIndicator + value));
                OnDataChanges(holder, path);
            });
        }

        private void DeleteChildren(ILocalDatabase localDatabase, LocalDatabaseEventHolder holder, bool includeSelf, string[] path, string serializedPath)
        {
            rwLock.LockRead(path, () =>
            {
                string childData = DBGet(localDatabase, serializedPath);
                if (childData != null)
                {
                    if (childData.Length > 1 && childData[0] == PathIndicator)
                    {
                        string[] deserialized = StringUtilities.Deserialize(childData.Substring(1));
                        if (deserialized != null)
                        {
                            foreach (string deserializedChildPath in deserialized)
                            {
                                string[] nextChild = new string[path.Length + 1];
                                nextChild[nextChild.Length - 1] = deserializedChildPath;
                                Array.Copy(path, 0, nextChild, 0, path.Length);
                                string serializedChildPath = StringUtilities.Serialize(nextChild);
                                DeleteChildren(localDatabase, holder, true, nextChild, serializedChildPath);
                            }
                        }
                    }
                    if (includeSelf)
                    {
                        rwLock.LockWrite(path, () => DBDelete(localDatabase, serializedPath));
                        OnDataChanges(holder, path);
                    }
                }
            });
        }

        private void OnDataChanges(LocalDatabaseEventHolder holder, string[] path)
        {
            holder?.Invoke(new DataChangesEventArgs(path));
        }

        private LocalDatabaseEventHolder GetHandler(ILocalDatabase localDatabase)
        {
            return databaseDictionaryLock.LockRead(() =>
            {
                databaseDictionary.TryGetValue(localDatabase, out LocalDatabaseEventHolder holder);
                return holder;
            });
        }

        private void LockReadHierarchy(string[] path, Action action)
        {
            void read(int index)
            {
                if (index >= path.Length)
                {
                    action();
                }
                else
                {
                    int nextIndex = index + 1;

                    string[] pathToLock = new string[nextIndex];
                    Array.Copy(path, 0, pathToLock, 0, nextIndex);

                    rwLock.LockRead(pathToLock, () => read(nextIndex));
                }
            }
            read(0);
        }

        private TReturn LockReadHierarchy<TReturn>(string[] path, Func<TReturn> block)
        {
            TReturn read(int index)
            {
                if (index >= path.Length)
                {
                    return block();
                }
                else
                {
                    int nextIndex = index + 1;

                    string[] pathToLock = new string[nextIndex];
                    Array.Copy(path, 0, pathToLock, 0, nextIndex);

                    return rwLock.LockRead(pathToLock, () => read(nextIndex));
                }
            }
            return read(0);
        }

        private void Validate(ILocalDatabase localDatabase, string[] path)
        {
            if (localDatabase == null)
            {
                throw new ArgumentNullException(nameof(localDatabase));
            }
            if (path == null || path?.Length == 0)
            {
                throw StringNullOrEmptyException.FromSingleArgument(nameof(path));
            }
            if (path.Any(i => string.IsNullOrEmpty(i)))
            {
                throw StringNullOrEmptyException.FromEnumerableArgument(nameof(path));
            }
        }

        #endregion

        #region DB Methods

        private void DBClear(ILocalDatabase localDatabase)
        {
            localDatabase.Clear();
        }

        private bool DBContains(ILocalDatabase localDatabase, string key)
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

        private string DBGet(ILocalDatabase localDatabase, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = null;

            encryptedValue = localDatabase.Get(encryptedKey);

            return App.Config.LocalEncryption.DecryptValue(encryptedValue);
        }

        private void DBSet(ILocalDatabase localDatabase, string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string encryptedKey = App.Config.LocalEncryption.EncryptKey(key);
            string encryptedValue = App.Config.LocalEncryption.EncryptValue(value);

            localDatabase.Set(encryptedKey, encryptedValue);
        }

        #endregion

        #region Helper Classes

        private class LocalDatabaseEventHolder
        {
            public event EventHandler<DataChangesEventArgs> Changes;

            private RWLock rwLock = new RWLock(LockRecursionPolicy.SupportsRecursion);
            private LocalDatabaseApp localDatabaseApp;
            private ConcurrentQueue<DataChangesEventArgs> invokes = new ConcurrentQueue<DataChangesEventArgs>();
            private bool isInvoking = false;

            public LocalDatabaseEventHolder(LocalDatabaseApp localDatabaseApp, EventHandler<DataChangesEventArgs> initialChangesHandler)
            {
                this.localDatabaseApp = localDatabaseApp;
                Changes += initialChangesHandler;
            }

            public void Invoke(DataChangesEventArgs args)
            {
                invokes.Enqueue(args);

                if (!isInvoking)
                {
                    isInvoking = true;

                    Task.Run(delegate
                    {
                        while (invokes.TryDequeue(out DataChangesEventArgs argsToInvoke))
                        {
                            Changes?.Invoke(localDatabaseApp, argsToInvoke);
                        }
                        isInvoking = false;
                    });
                }
            }
        }

        #endregion
    }
}
