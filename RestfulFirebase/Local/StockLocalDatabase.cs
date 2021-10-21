using System.Collections.Concurrent;

namespace RestfulFirebase.Local
{
    /// <summary>
    /// The provided stock <see cref="ILocalDatabase"/> implementation to be used.
    /// </summary>
    public sealed class StockLocalDatabase : ILocalDatabase
    {
        private static volatile ConcurrentDictionary<string, string> db = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Creates new instance of <see cref="StockLocalDatabase"/> class.
        /// </summary>
        public StockLocalDatabase()
        {

        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        /// <inheritdoc/>
        public string Get(string key)
        {
            if (!db.TryGetValue(key, out string value))
            {
                return null;
            }
            return value;
        }

        /// <inheritdoc/>
        public void Set(string key, string value)
        {
            db.AddOrUpdate(key, value, delegate { return value; });
        }

        /// <inheritdoc/>
        public void Delete(string key)
        {
            db.TryRemove(key, out _);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            db.Clear();
        }
    }
}
