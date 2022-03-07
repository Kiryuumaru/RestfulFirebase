// Partial locking mechianism

using LockerHelpers;
using ObservableHelpers;
using ObservableHelpers.Utilities;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using SerializerHelpers;
using SynchronizationContextHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Local;

/// <summary>
/// App module that provides persistency for the <see cref="RestfulFirebaseApp"/>.
/// </summary>
public class LocalDatabaseApp : SyncContext
{
    #region Properties

    /// <inheritdoc/>
    public RestfulFirebaseApp App { get; }

    private const char ValueIndicator = 'v';
    private const char PathIndicator = 'p';

    private readonly RWLockDictionary<string[]> rwLock = new(LockRecursionPolicy.SupportsRecursion, PathEqualityComparer.Instance);
    private static readonly RWLock databaseDictionaryLock = new(LockRecursionPolicy.SupportsRecursion);
    private static readonly Dictionary<ILocalDatabase, LocalDatabaseEventHolder> databaseDictionary = new();

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
        InternalSubscribe(App.Config.CachedLocalDatabase, changesHandler);
    }

    /// <summary>
    /// Unsubscribe to local database changes.
    /// </summary>
    /// <param name="changesHandler">
    /// The handler to unsubscribe.
    /// </param>
    public void Unsubscribe(EventHandler<DataChangesEventArgs> changesHandler)
    {
        InternalUnsubscribe(App.Config.CachedLocalDatabase, changesHandler);
    }

    /// <summary>
    /// Check if the specified <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">
    /// The path to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <paramref name="path"/> exists; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool Contains(params string[] path)
    {
        return InternalContains(App.Config.CachedLocalDatabase, path);
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
        InternalDelete(App.Config.CachedLocalDatabase, path);
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
        return InternalGetChildren(App.Config.CachedLocalDatabase, path);
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
        return InternalGetDataType(App.Config.CachedLocalDatabase, path);
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
        return InternalGetRecursiveChildren(App.Config.CachedLocalDatabase, path);
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
        return InternalGetRecursiveRelativeChildren(App.Config.CachedLocalDatabase, path);
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
        return InternalGetRelativeTypedChildren(App.Config.CachedLocalDatabase, path);
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
        return InternalGetTypedChildren(App.Config.CachedLocalDatabase, path);
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
    public string? GetValue(params string[] path)
    {
        return InternalGetValue(App.Config.CachedLocalDatabase, path);
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
        InternalSetValue(App.Config.CachedLocalDatabase, value, path);
    }

    /// <summary>
    /// Gets the value or children of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrChildren(Action<string> onValue, Action<(string[] path, string key)[]> onPath, params string[] path)
    {
        return InternalTryGetValueOrChildren(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains another path.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrPath(Action<string> onValue, Action onPath, params string[] path)
    {
        return InternalTryGetValueOrPath(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or recursive children of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrRecursiveChildren(Action<string> onValue, Action<string[][]> onPath, params string[] path)
    {
        return InternalTryGetValueOrRecursiveChildren(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or recursive relative children of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrRecursiveRelativeChildren(Action<string> onValue, Action<string[][]> onPath, params string[] path)
    {
        return InternalTryGetValueOrRecursiveRelativeChildren(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or recursive values of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrRecursiveValues(Action<string> onValue, Action<(string[] path, string value)[]> onPath, params string[] path)
    {
        return InternalTryGetValueOrRecursiveValues(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or recursive relative values of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrRecursiveRelativeValues(Action<string> onValue, Action<(string[] path, string value)[]> onPath, params string[] path)
    {
        return InternalTryGetValueOrRecursiveRelativeValues(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or relative typed children of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrRelativeTypedChildren(Action<string> onValue, Action<(string key, LocalDataType type)[]> onPath, params string[] path)
    {
        return InternalTryGetValueOrRelativeTypedChildren(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    /// <summary>
    /// Gets the value or typed children of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="onValue">
    /// Action executed whether the <paramref name="path"/> contains a value.
    /// </param>
    /// <param name="onPath">
    /// Action executed whether the <paramref name="path"/> contains children.
    /// </param>
    /// <param name="path">
    /// The path to get.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the path contains value or path; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    public bool TryGetValueOrTypedChildren(Action<string> onValue, Action<(string[] path, LocalDataType type)[]> onPath, params string[] path)
    {
        return InternalTryGetValueOrTypedChildren(App.Config.CachedLocalDatabase, onValue, onPath, path);
    }

    #endregion

    #region Internal Implementations

    internal void InternalSubscribe(ILocalDatabase localDatabase, EventHandler<DataChangesEventArgs> changesHandler)
    {
        databaseDictionaryLock.LockUpgradeableRead(() =>
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
        databaseDictionaryLock.LockUpgradeableRead(() =>
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
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);

        return LockReadHierarchy(path, () => DBContains(localDatabase, serializedPath));
    }

    internal void InternalDelete(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);

        LockReadUpgradableHierarchy(path, () =>
        {
            LocalDatabaseEventHolder? holder = LocalDatabaseApp.GetHandler(localDatabase);

            HelperDeleteChildren(localDatabase, holder, true, path, serializedPath);

            string[] hierPath = path;
            string? lastNode = null;

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

                string serializedHierPath = StringSerializer.Serialize(hierPath);
                string? hierData = DBGet(localDatabase, serializedHierPath);
                if (hierData != null)
                {
                    if (hierData.Length > 1 && hierData[0] == PathIndicator)
                    {
                        string?[]? deserialized = StringSerializer.Deserialize(hierData[1..]);
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
                                    string?[] modified = deserialized.RemoveAt(indexOf);
                                    string data = PathIndicator + StringSerializer.Serialize(modified);
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
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        string? data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

        return HelperGetChildren(path, data);
    }

    internal LocalDataType InternalGetDataType(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        string? data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            return LocalDataType.Path;
        }
        else
        {
            return LocalDataType.Value;
        }
    }

    internal (string[] path, LocalDataType type)[] InternalGetTypedChildren(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(localDatabase, serializedPath);

            return HelperGetTypedChildren(localDatabase, path, data);
        });

    }

    internal string[][] InternalGetRecursiveChildren(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(localDatabase, serializedPath);

            return HelperGetRecursiveChildren(localDatabase, path, data);
        });
    }

    internal string[][] InternalGetRecursiveRelativeChildren(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(localDatabase, serializedPath);

            return HelperGetRecursiveRelativeChildren(localDatabase, path, data);
        });
    }

    internal (string key, LocalDataType type)[] InternalGetRelativeTypedChildren(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(localDatabase, serializedPath);

            return HelperGetRelativeTypedChildren(localDatabase, path, data);
        });
    }

    internal string? InternalGetValue(ILocalDatabase localDatabase, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);

        string? data = LockReadHierarchy(path, () => DBGet(localDatabase, serializedPath));

        if (data != null && data.Length > 0)
        {
            return data[0] == ValueIndicator ? data[1..] : default;
        }
        else
        {
            return default;
        }
    }

    internal void InternalSetValue(ILocalDatabase localDatabase, string? value, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);
        LockReadUpgradableHierarchy(path, () =>
        {
            LocalDatabaseEventHolder? holder = LocalDatabaseApp.GetHandler(localDatabase);

            HelperDeleteChildren(localDatabase, holder, false, path, serializedPath);

            if (path.Length > 1)
            {
                (string[] path, string serializedPath)[] absolutePaths = new (string[] path, string serializedPath)[path.Length - 1];
                int startIndex = path.Length - 2;
                string? lastValueToSet = null;
                bool skipLast = false;

                for (int i = path.Length - 2; i >= 0; i--)
                {
                    int nextI = i + 1;
                    string[] keyHier = new string[nextI];
                    Array.Copy(path, 0, keyHier, 0, nextI);
                    string serializedKeyHier = StringSerializer.Serialize(keyHier);

                    startIndex = i;
                    absolutePaths[i] = (keyHier, serializedKeyHier);

                    string? data = DBGet(localDatabase, serializedKeyHier);
                    if (data != null && data.Length > 0 && data[0] == PathIndicator)
                    {
                        string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
                        if (deserialized != null && deserialized.Length != 0)
                        {
                            if (!deserialized.Contains(path[nextI]))
                            {
                                string[] modified = new string[deserialized.Length + 1];
                                Array.Copy(deserialized, 0, modified, 0, deserialized.Length);
                                modified[^1] = path[nextI];
                                lastValueToSet = PathIndicator + StringSerializer.Serialize(modified);
                                skipLast = true;
                                break;
                            }
                            else
                            {
                                skipLast = true;
                                break;
                            }
                        }
                    }
                }

                if (lastValueToSet != null)
                {
                    rwLock.LockWrite(
                        absolutePaths[startIndex].path,
                        () => DBSet(localDatabase, absolutePaths[startIndex].serializedPath, lastValueToSet));
                    OnDataChanges(holder, absolutePaths[startIndex].path);
                }

                for (int i = skipLast ? startIndex + 1 : startIndex; i < absolutePaths.Length; i++)
                {
                    string valueToSet = PathIndicator + StringSerializer.Serialize(new string[] { path[i + 1] });
                    rwLock.LockWrite(absolutePaths[i].path,
                        () => DBSet(localDatabase, absolutePaths[i].serializedPath, valueToSet));
                    OnDataChanges(holder, absolutePaths[i].path);
                }
            }

            rwLock.LockWrite(path, () => DBSet(localDatabase, serializedPath, ValueIndicator + value));
            OnDataChanges(holder, path);
        });
    }

    internal void InternalTryGetNearestHierarchyValueOrPath(ILocalDatabase localDatabase, Action<(string[] path, string? value)> onValue, Action<string[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        string serializedPath = StringSerializer.Serialize(path);

        LockReadHierarchy(path, () =>
        {
            string[] hierPath = path;

            while (true)
            {
                string serializedHierPath = StringSerializer.Serialize(hierPath);
                string? hierData = DBGet(localDatabase, serializedHierPath);

                if (hierData != null)
                {
                    if (hierData.Length > 1)
                    {
                        if (hierData[0] == PathIndicator)
                        {
                            onPath?.Invoke(hierPath);
                            return;
                        }
                        else if (hierData[0] == ValueIndicator)
                        {
                            onValue?.Invoke((hierPath, hierData[1..]));
                            return;
                        }
                    }
                }

                if (hierPath.Length > 1)
                {
                    int newHierLength = hierPath.Length - 1;
                    hierPath = new string[newHierLength];
                    Array.Copy(path, 0, hierPath, 0, newHierLength);
                }
                else
                {
                    break;
                }
            }
        });
    }

    internal bool InternalTryGetValueOrChildren(ILocalDatabase localDatabase, Action<string> onValue, Action<(string[] path, string key)[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetChildren(path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrPath(ILocalDatabase localDatabase, Action<string> onValue, Action onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke();
        }, path);
    }

    internal bool InternalTryGetValueOrRecursiveChildren(ILocalDatabase localDatabase, Action<string> onValue, Action<string[][]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveChildren(localDatabase, path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrRecursiveRelativeChildren(ILocalDatabase localDatabase, Action<string> onValue, Action<string[][]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveRelativeChildren(localDatabase, path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrRecursiveValues(ILocalDatabase localDatabase, Action<string> onValue, Action<(string[] path, string value)[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveValues(localDatabase, path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrRecursiveRelativeValues(ILocalDatabase localDatabase, Action<string> onValue, Action<(string[] path, string value)[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveRelativeValues(localDatabase, path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrRelativeTypedChildren(ILocalDatabase localDatabase, Action<string> onValue, Action<(string key, LocalDataType type)[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRelativeTypedChildren(localDatabase, path, p.data));
        }, path);
    }

    internal bool InternalTryGetValueOrTypedChildren(ILocalDatabase localDatabase, Action<string> onValue, Action<(string[] path, LocalDataType type)[]> onPath, string[] path)
    {
        LocalDatabaseApp.Validate(localDatabase, path);

        return HelperTryGetValueOrPath(localDatabase, v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetTypedChildren(localDatabase, path, p.data));
        }, path);
    }

    #endregion

    #region Helper Methods

    private bool HelperTryGetValueOrPath(ILocalDatabase localDatabase, Action<(string value, string serializedPath)> onValue, Action<(string data, string serializedPath)> onPath, string[] path)
    {
        string serializedPath = StringSerializer.Serialize(path);

        return LockReadHierarchy(path, () =>
        {
            string? data = DBGet(localDatabase, serializedPath);

            if (data != null && data.Length > 0)
            {
                if (data[0] == ValueIndicator)
                {
                    onValue?.Invoke((data[1..], serializedPath));
                    return true;
                }
                else if (data[0] == PathIndicator)
                {
                    onPath?.Invoke((data, serializedPath));
                    return true;
                }
            }

            return false;
        });
    }
    
    private static (string[] path, string key)[] HelperGetChildren(string[] path, string? data)
    {
        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                (string[] path, string key)[] paths = new (string[] path, string key)[deserialized.Length];
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    paths[i] = (subPath, currentPath);
                }
                return paths;
            }
        }
        return Array.Empty<(string[] path, string key)>();
    }

    private (string[] path, LocalDataType type)[] HelperGetTypedChildren(ILocalDatabase localDatabase, string[] path, string? data)
    {
        List<(string[] path, LocalDataType type)> paths = new();

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(subPath, () =>
                    {
                        string? subData = DBGet(localDatabase, serializedSubPath);

                        if (subData != null && subData.Length > 0 && subData[0] == PathIndicator)
                        {
                            paths.Add((subPath, LocalDataType.Path));
                        }
                        else
                        {
                            paths.Add((subPath, LocalDataType.Value));
                        }
                    });
                }
            }
        }

        return paths.ToArray();
    }

    private (string key, LocalDataType type)[] HelperGetRelativeTypedChildren(ILocalDatabase localDatabase, string[] path, string? data)
    {
        List<(string key, LocalDataType type)> paths = new();

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(subPath, () =>
                    {
                        string? subData = DBGet(localDatabase, serializedSubPath);

                        if (subData != null && subData.Length > 0 && subData[0] == PathIndicator)
                        {
                            paths.Add((subPath[^1], LocalDataType.Path));
                        }
                        else
                        {
                            paths.Add((subPath[^1], LocalDataType.Value));
                        }
                    });
                }
            }
        }

        return paths.ToArray();
    }

    private string[][] HelperGetRecursiveChildren(ILocalDatabase localDatabase, string[] path, string? data)
    {
        List<string[]> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(localDatabase, serializedRecvPath);

            if (recvData != null && recvData.Length > 0 && recvData[0] == PathIndicator)
            {
                string?[]? deserialized = StringSerializer.Deserialize(recvData[1..]);
                if (deserialized != null)
                {
                    for (int i = 0; i < deserialized.Length; i++)
                    {
                        string? currentPath = deserialized[i];
                        if (currentPath == null)
                        {
                            throw new Exception();
                        }
                        string[] subPath = new string[recvPath.Length + 1];
                        subPath[^1] = currentPath;
                        Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                        string serializedSubPath = StringSerializer.Serialize(subPath);
                        rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                    }
                }
            }
            else
            {
                paths.Add(recvPath);
            }
        }

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(path, () => recursive(subPath, serializedSubPath, 1));
                }
            }
        }

        return paths.ToArray();
    }

    private string[][] HelperGetRecursiveRelativeChildren(ILocalDatabase localDatabase, string[] path, string? data)
    {
        List<string[]> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(localDatabase, serializedRecvPath);

            if (recvData != null && recvData.Length > 0 && recvData[0] == PathIndicator)
            {
                string?[]? deserialized = StringSerializer.Deserialize(recvData[1..]);
                if (deserialized != null)
                {
                    for (int i = 0; i < deserialized.Length; i++)
                    {
                        string? currentPath = deserialized[i];
                        if (currentPath == null)
                        {
                            throw new Exception();
                        }
                        string[] subPath = new string[recvPath.Length + 1];
                        subPath[^1] = currentPath;
                        Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                        string serializedSubPath = StringSerializer.Serialize(subPath);
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

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(path, () => recursive(subPath, serializedSubPath, 1));
                }
            }
        }

        return paths.ToArray();
    }

    private (string[] path, string value)[] HelperGetRecursiveValues(ILocalDatabase localDatabase, string[] path, string data)
    {
        List<(string[] path, string value)> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(localDatabase, serializedRecvPath);

            if (recvData != null && recvData.Length > 0 && recvData[0] == PathIndicator)
            {
                string?[]? deserialized = StringSerializer.Deserialize(recvData[1..]);
                if (deserialized != null)
                {
                    for (int i = 0; i < deserialized.Length; i++)
                    {
                        string? currentPath = deserialized[i];
                        if (currentPath == null)
                        {
                            throw new Exception();
                        }
                        string[] subPath = new string[recvPath.Length + 1];
                        subPath[^1] = currentPath;
                        Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                        string serializedSubPath = StringSerializer.Serialize(subPath);
                        rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                    }
                }
            }
            else if (recvData != null)
            {
                paths.Add((recvPath, recvData[1..]));
            }
        }

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(path, () => recursive(subPath, serializedSubPath, 1));
                }
            }
        }

        return paths.ToArray();
    }

    private (string[] path, string value)[] HelperGetRecursiveRelativeValues(ILocalDatabase localDatabase, string[] path, string data)
    {
        List<(string[] path, string value)> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(localDatabase, serializedRecvPath);

            if (recvData != null && recvData.Length > 0 && recvData[0] == PathIndicator)
            {
                string?[]? deserialized = StringSerializer.Deserialize(recvData[1..]);
                if (deserialized != null)
                {
                    for (int i = 0; i < deserialized.Length; i++)
                    {
                        string? currentPath = deserialized[i];
                        if (currentPath == null)
                        {
                            throw new Exception();
                        }
                        string[] subPath = new string[recvPath.Length + 1];
                        subPath[^1] = currentPath;
                        Array.Copy(recvPath, 0, subPath, 0, recvPath.Length);
                        string serializedSubPath = StringSerializer.Serialize(subPath);
                        rwLock.LockRead(recvPath, () => recursive(subPath, serializedSubPath, nextRoot));
                    }
                }
            }
            else if (recvData != null && recvData.Length > 0 && recvData[0] == ValueIndicator)
            {
                string[] pathToAdd = new string[root];
                Array.Copy(recvPath, recvPath.Length - root, pathToAdd, 0, root);
                paths.Add((pathToAdd, recvData[1..]));
            }
        }

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            string?[]? deserialized = StringSerializer.Deserialize(data[1..]);
            if (deserialized != null)
            {
                for (int i = 0; i < deserialized.Length; i++)
                {
                    string? currentPath = deserialized[i];
                    if (currentPath == null)
                    {
                        throw new Exception();
                    }
                    string[] subPath = new string[path.Length + 1];
                    subPath[^1] = currentPath;
                    Array.Copy(path, 0, subPath, 0, path.Length);
                    string serializedSubPath = StringSerializer.Serialize(subPath);
                    rwLock.LockRead(path, () => recursive(subPath, serializedSubPath, 1));
                }
            }
        }

        return paths.ToArray();
    }

    private void HelperDeleteChildren(ILocalDatabase localDatabase, LocalDatabaseEventHolder? holder, bool includeSelf, string[] path, string serializedPath)
    {
        string? childData = DBGet(localDatabase, serializedPath);
        if (childData != null)
        {
            if (childData.Length > 1 && childData[0] == PathIndicator)
            {
                string?[]? deserialized = StringSerializer.Deserialize(childData[1..]);
                if (deserialized != null)
                {
                    foreach (string? deserializedChildPath in deserialized)
                    {
                        if (deserializedChildPath == null)
                        {
                            throw new Exception();
                        }
                        string[] nextChild = new string[path.Length + 1];
                        nextChild[^1] = deserializedChildPath;
                        Array.Copy(path, 0, nextChild, 0, path.Length);
                        string serializedChildPath = StringSerializer.Serialize(nextChild);
                        rwLock.LockUpgradeableRead(path, () => HelperDeleteChildren(localDatabase, holder, true, nextChild, serializedChildPath));
                    }
                }
            }
            if (includeSelf)
            {
                rwLock.LockWrite(path, () => DBDelete(localDatabase, serializedPath));
                OnDataChanges(holder, path);
            }
        }
    }

    private static void OnDataChanges(LocalDatabaseEventHolder? holder, string[] path)
    {
        holder?.Invoke(new DataChangesEventArgs(path));
    }

    private static LocalDatabaseEventHolder? GetHandler(ILocalDatabase localDatabase)
    {
        return databaseDictionaryLock.LockRead(() =>
        {
            databaseDictionary.TryGetValue(localDatabase, out LocalDatabaseEventHolder? holder);
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

    private void LockReadUpgradableHierarchy(string[] path, Action action)
    {
        LockReadUpgradableHierarchy(path, () =>
        {
            action();
            return 0;
        });
    }

    private TReturn LockReadUpgradableHierarchy<TReturn>(string[] path, Func<TReturn> block)
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

                return rwLock.LockUpgradeableRead(pathToLock, () => read(nextIndex));
            }
        }
        return read(0);
    }

    private static void Validate(ILocalDatabase localDatabase, string[] path)
    {
        if (localDatabase == null)
        {
            throw new ArgumentNullException(nameof(localDatabase));
        }
        if (path == null || path.Length == 0)
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

    private bool DBContains(ILocalDatabase localDatabase, string key)
    {
        if (App.Config.CachedLocalEncryption == null)
        {
            return localDatabase.ContainsKey(key);
        }
        else
        {
            string? encryptedKey = App.Config.CachedLocalEncryption.Encrypt(key);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return localDatabase.ContainsKey(encryptedKey);
        }
    }

    private void DBDelete(ILocalDatabase localDatabase, string key)
    {
        if (App.Config.CachedLocalEncryption == null)
        {
            localDatabase.Delete(key);
        }
        else
        {
            string? encryptedKey = App.Config.CachedLocalEncryption.Encrypt(key);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            localDatabase.Delete(encryptedKey);
        }
    }

    private string? DBGet(ILocalDatabase localDatabase, string key)
    {
        if (App.Config.CachedLocalEncryption == null)
        {
            return localDatabase.Get(key);
        }
        else
        {
            string? encryptedKey = App.Config.CachedLocalEncryption.Encrypt(key);
            string? encryptedValue;

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            encryptedValue = localDatabase.Get(encryptedKey);

            return App.Config.CachedLocalEncryption.Decrypt(encryptedValue);
        }
    }

    private void DBSet(ILocalDatabase localDatabase, string key, string value)
    {
        if (App.Config.CachedLocalEncryption == null)
        {
            localDatabase.Set(key, value);
        }
        else
        {
            string? encryptedKey = App.Config.CachedLocalEncryption.Encrypt(key);
            string? encryptedValue = App.Config.CachedLocalEncryption.Encrypt(value);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            localDatabase.Set(encryptedKey, encryptedValue);
        }
    }

    #endregion

    #region Helper Classes

    private class LocalDatabaseEventHolder
    {
        public event EventHandler<DataChangesEventArgs> Changes;

        private readonly LocalDatabaseApp localDatabaseApp;
        private readonly ConcurrentQueue<DataChangesEventArgs> invokes = new();
        private bool isInvoking = false;

        public LocalDatabaseEventHolder(LocalDatabaseApp localDatabaseApp, EventHandler<DataChangesEventArgs> initialChangesHandler)
        {
            this.localDatabaseApp = localDatabaseApp;
            Changes += initialChangesHandler;
        }

        public void Invoke(DataChangesEventArgs args)
        {
            //Changes?.Invoke(localDatabaseApp, args);
            invokes.Enqueue(args);

            if (!isInvoking)
            {
                isInvoking = true;

                Task.Run(delegate
                {
                    while (invokes.TryDequeue(out DataChangesEventArgs? argsToInvoke))
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
