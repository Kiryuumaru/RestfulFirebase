using System.Collections.Concurrent;

namespace RestfulFirebase.Local;

/// <summary>
/// The provided stock <see cref="ILocalDatabase"/> implementation to be used.
/// </summary>
public sealed class StockLocalDatabase : ILocalDatabase
{
    private ConcurrentDictionary<string, string?> Db { get; } = new();

    /// <summary>
    /// Creates new instance of <see cref="StockLocalDatabase"/> class.
    /// </summary>
    public StockLocalDatabase()
    {

    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return Db.ContainsKey(key);
    }

    /// <inheritdoc/>
    public string? Get(string key)
    {
        if (!Db.TryGetValue(key, out string? value))
        {
            return null;
        }
        return value;
    }

    /// <inheritdoc/>
    public void Set(string key, string? value)
    {
        Db.AddOrUpdate(key, value, delegate { return value; });
    }

    /// <inheritdoc/>
    public void Delete(string key)
    {
        Db.TryRemove(key, out _);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Db.Clear();
    }
}
