// Partial locking mechianism

using DisposableHelpers;
using DisposableHelpers.Attributes;
using LockerHelpers;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Local;

/// <summary>
/// App module that provides persistency for the <see cref="RestfulFirebaseApp"/>.
/// </summary>
[Disposable]
public partial class LocalDatabaseApp
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Event that invokes if the local database has data changes.
    /// </summary>
    public event EventHandler<DataChangesEventArgs>? Changes;

    private bool isChangesInvoking = false;

    private const char ValueIndicator = 'v';
    private const char PathIndicator = 'p';
    private const string ExposedStoreIndicator = "exdb";

    private readonly RWLockDictionary<string[]> rwLock = new(LockRecursionPolicy.SupportsRecursion, PathEqualityComparer.Instance);
    private readonly ConcurrentDictionary<string, object?> nonPersistentStore = new();
    private readonly ConcurrentQueue<DataChangesEventArgs> changesInvokes = new();

    #endregion

    #region Initializers

    internal LocalDatabaseApp(RestfulFirebaseApp app)
    {
        App = app;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets a value of the specified <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to set.
    /// </typeparam>
    /// <param name="key">
    /// The key of the value to set.
    /// </param>
    /// <param name="value">
    /// The value to set.
    /// </param>
    /// <param name="fromPersistentStore">
    /// <c>true</c> if the value will set to the persistent store; otherwise, <c>false</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is null or empty.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/> for <typeparamref name="T"/> or its serializable members.
    /// </exception>
    public void SetValue<T>(string key, T value, bool fromPersistentStore)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Parameter key is null or empty");
        }
        if (fromPersistentStore)
        {
            string? serialized = JsonSerializer.Serialize(value, App.Config.DatabaseJsonSerializerOptions);
            SetValue(serialized, new string[] { ExposedStoreIndicator, key });
        }
        else
        {
            nonPersistentStore.AddOrUpdate(key, value, (_, _) => value);
        }
    }

    /// <summary>
    /// Gets a value of the specified <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to get.
    /// </typeparam>
    /// <param name="key">
    /// The key of the value to get.
    /// </param>
    /// <param name="fromPersistentStore">
    /// <c>true</c> if the value will get from the persistent store; otherwise, <c>false</c>.
    /// </param>
    /// <returns>
    /// The value of the specified <paramref name="key"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is null or empty.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/> for <typeparamref name="T"/> or its serializable members.
    /// </exception>
    public T? GetValue<T>(string key, bool fromPersistentStore)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Parameter key is null or empty");
        }
        if (fromPersistentStore)
        {
            string? serialized = GetValue(new string[] { ExposedStoreIndicator, key });
            return serialized == null ? default : JsonSerializer.Deserialize<T>(serialized, App.Config.DatabaseJsonSerializerOptions);
        }
        else
        {
            if (nonPersistentStore.TryGetValue(key, out object? value))
            {
                if (value != null)
                {
                    return (T)value;
                }
            }
            return default;
        }
    }

    /// <summary>
    /// Deletes a value of the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">
    /// The key of the value to delete.
    /// </param>
    /// <param name="fromPersistentStore">
    /// <c>true</c> if the value will delete from the persistent store; otherwise, <c>false</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is null or empty.
    /// </exception>
    public void RemoveValue(string key, bool fromPersistentStore)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Parameter key is null or empty");
        }
        if (fromPersistentStore)
        {
            Delete(new string[] { ExposedStoreIndicator, key });
        }
        else
        {
            nonPersistentStore.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Checks a value of the specified <paramref name="key"/>, gets <c>true</c> if exists; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="key">
    /// The key of the value to check.
    /// </param>
    /// <param name="fromPersistentStore">
    /// <c>true</c> if the value will check from the persistent store; otherwise, <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value exists; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key"/> is null or empty.
    /// </exception>
    public bool ContainsKey(string key, bool fromPersistentStore)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Parameter key is null or empty");
        }
        if (fromPersistentStore)
        {
            return Contains(new string[] { ExposedStoreIndicator, key });
        }
        else
        {
            return nonPersistentStore.ContainsKey(key);
        }
    }

    #endregion

    #region Internal Implementations

    internal bool Contains(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);

        return LockReadHierarchy(path, () => DBContains(serializedPath));
    }

    internal void Delete(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);

        LockReadUpgradableHierarchy(path, () =>
        {
            HelperDeleteChildren(true, path, serializedPath);

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
                string? hierData = DBGet(serializedHierPath);
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
                                    rwLock.LockWrite(hierPath, () => DBDelete(serializedHierPath));
                                    OnDataChanges(hierPath);
                                }
                                else
                                {
                                    string?[] modified = deserialized.RemoveAt(indexOf);
                                    string data = PathIndicator + StringSerializer.Serialize(modified);
                                    rwLock.LockWrite(hierPath, () => DBSet(serializedHierPath, data));
                                    OnDataChanges(hierPath);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        rwLock.LockWrite(hierPath, () => DBDelete(serializedHierPath));
                        OnDataChanges(hierPath);
                    }
                }
            }
        });
    }

    internal (string[] path, string key)[] GetChildren(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        string? data = LockReadHierarchy(path, () => DBGet(serializedPath));

        return HelperGetChildren(path, data);
    }

    internal LocalDataType GetDataType(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        string? data = LockReadHierarchy(path, () => DBGet(serializedPath));

        if (data != null && data.Length > 0 && data[0] == PathIndicator)
        {
            return LocalDataType.Path;
        }
        else
        {
            return LocalDataType.Value;
        }
    }

    internal (string[] path, LocalDataType type)[] GetTypedChildren(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(serializedPath);

            return HelperGetTypedChildren(path, data);
        });

    }

    internal string[][] GetRecursiveChildren(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(serializedPath);

            return HelperGetRecursiveChildren(path, data);
        });
    }

    internal string[][] GetRecursiveRelativeChildren(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(serializedPath);

            return HelperGetRecursiveRelativeChildren(path, data);
        });
    }

    internal (string key, LocalDataType type)[] GetRelativeTypedChildren(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        return rwLock.LockRead(path, () =>
        {
            string? data = DBGet(serializedPath);

            return HelperGetRelativeTypedChildren(path, data);
        });
    }

    internal string? GetValue(params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);

        string? data = LockReadHierarchy(path, () => DBGet(serializedPath));

        if (data != null && data.Length > 0)
        {
            return data[0] == ValueIndicator ? data[1..] : default;
        }
        else
        {
            return default;
        }
    }

    internal void SetValue(string? value, params string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);
        LockReadUpgradableHierarchy(path, () =>
        {
            HelperDeleteChildren(false, path, serializedPath);

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

                    string? data = DBGet(serializedKeyHier);
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
                        () => DBSet(absolutePaths[startIndex].serializedPath, lastValueToSet));
                    OnDataChanges(absolutePaths[startIndex].path);
                }

                for (int i = skipLast ? startIndex + 1 : startIndex; i < absolutePaths.Length; i++)
                {
                    string valueToSet = PathIndicator + StringSerializer.Serialize(new string[] { path[i + 1] });
                    rwLock.LockWrite(absolutePaths[i].path,
                        () => DBSet(absolutePaths[i].serializedPath, valueToSet));
                    OnDataChanges(absolutePaths[i].path);
                }
            }

            rwLock.LockWrite(path, () => DBSet(serializedPath, ValueIndicator + value));
            OnDataChanges(path);
        });
    }

    internal void InternalTryGetNearestHierarchyValueOrPath(Action<(string[] path, string? value)> onValue, Action<string[]> onPath, string[] path)
    {
        Validate(path);

        string serializedPath = StringSerializer.Serialize(path);

        LockReadHierarchy(path, () =>
        {
            string[] hierPath = path;

            while (true)
            {
                string serializedHierPath = StringSerializer.Serialize(hierPath);
                string? hierData = DBGet(serializedHierPath);

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

    internal bool TryGetValueOrChildren(Action<string> onValue, Action<(string[] path, string key)[]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetChildren(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrPath(Action<string> onValue, Action onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke();
        }, path);
    }

    internal bool TryGetValueOrRecursiveChildren(Action<string> onValue, Action<string[][]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveChildren(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrRecursiveRelativeChildren(Action<string> onValue, Action<string[][]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveRelativeChildren(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrRecursiveValues(Action<string> onValue, Action<(string[] path, string value)[]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveValues(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrRecursiveRelativeValues(Action<string> onValue, Action<(string[] path, string value)[]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRecursiveRelativeValues(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrRelativeTypedChildren(Action<string> onValue, Action<(string key, LocalDataType type)[]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetRelativeTypedChildren(path, p.data));
        }, path);
    }

    internal bool TryGetValueOrTypedChildren(Action<string> onValue, Action<(string[] path, LocalDataType type)[]> onPath, params string[] path)
    {
        Validate(path);

        return HelperTryGetValueOrPath(v => onValue?.Invoke(v.value), p =>
        {
            onPath?.Invoke(HelperGetTypedChildren(path, p.data));
        }, path);
    }

    #endregion

    #region Helper Methods

    private bool HelperTryGetValueOrPath(Action<(string value, string serializedPath)> onValue, Action<(string data, string serializedPath)> onPath, string[] path)
    {
        string serializedPath = StringSerializer.Serialize(path);

        return LockReadHierarchy(path, () =>
        {
            string? data = DBGet(serializedPath);

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

    private (string[] path, LocalDataType type)[] HelperGetTypedChildren(string[] path, string? data)
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
                        string? subData = DBGet(serializedSubPath);

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

    private (string key, LocalDataType type)[] HelperGetRelativeTypedChildren(string[] path, string? data)
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
                        string? subData = DBGet(serializedSubPath);

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

    private string[][] HelperGetRecursiveChildren(string[] path, string? data)
    {
        List<string[]> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(serializedRecvPath);

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

    private string[][] HelperGetRecursiveRelativeChildren(string[] path, string? data)
    {
        List<string[]> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(serializedRecvPath);

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

    private (string[] path, string value)[] HelperGetRecursiveValues(string[] path, string data)
    {
        List<(string[] path, string value)> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(serializedRecvPath);

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

    private (string[] path, string value)[] HelperGetRecursiveRelativeValues(string[] path, string data)
    {
        List<(string[] path, string value)> paths = new();

        void recursive(string[] recvPath, string serializedRecvPath, int root)
        {
            int nextRoot = root + 1;

            string? recvData = DBGet(serializedRecvPath);

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

    private void HelperDeleteChildren(bool includeSelf, string[] path, string serializedPath)
    {
        string? childData = DBGet(serializedPath);
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
                        rwLock.LockUpgradeableRead(path, () => HelperDeleteChildren(true, nextChild, serializedChildPath));
                    }
                }
            }
            if (includeSelf)
            {
                rwLock.LockWrite(path, () => DBDelete(serializedPath));
                OnDataChanges(path);
            }
        }
    }

    private void OnDataChanges(string[] path)
    {
        //Changes?.Invoke(localDatabaseApp, args);
        changesInvokes.Enqueue(new DataChangesEventArgs(path));

        if (!isChangesInvoking)
        {
            isChangesInvoking = true;

            Task.Run(delegate
            {
                while (changesInvokes.TryDequeue(out DataChangesEventArgs? argsToInvoke))
                {
                    try
                    {
                        Changes?.Invoke(this, argsToInvoke);
                    }
                    catch { }
                }
                isChangesInvoking = false;
            });
        }
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

    private static void Validate(string[] path)
    {
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

    private bool DBContains(string key)
    {
        if (App.Config.LocalEncryption == null)
        {
            return App.Config.LocalDatabase.ContainsKey(key);
        }
        else
        {
            string? encryptedKey = App.Config.LocalEncryption.Encrypt(key);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return App.Config.LocalDatabase.ContainsKey(encryptedKey);
        }
    }

    private void DBDelete(string key)
    {
        if (App.Config.LocalEncryption == null)
        {
            App.Config.LocalDatabase.Delete(key);
        }
        else
        {
            string? encryptedKey = App.Config.LocalEncryption.Encrypt(key);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            App.Config.LocalDatabase.Delete(encryptedKey);
        }
    }

    private string? DBGet(string key)
    {
        if (App.Config.LocalEncryption == null)
        {
            return App.Config.LocalDatabase.Get(key);
        }
        else
        {
            string? encryptedKey = App.Config.LocalEncryption.Encrypt(key);
            string? encryptedValue;

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            encryptedValue = App.Config.LocalDatabase.Get(encryptedKey);

            return App.Config.LocalEncryption.Decrypt(encryptedValue);
        }
    }

    private void DBSet(string key, string value)
    {
        if (App.Config.LocalEncryption == null)
        {
            App.Config.LocalDatabase.Set(key, value);
        }
        else
        {
            string? encryptedKey = App.Config.LocalEncryption.Encrypt(key);
            string? encryptedValue = App.Config.LocalEncryption.Encrypt(value);

            if (encryptedKey == null || string.IsNullOrEmpty(encryptedKey))
            {
                throw new ArgumentNullException(nameof(key));
            }

            App.Config.LocalDatabase.Set(encryptedKey, encryptedValue);
        }
    }

    #endregion
}
