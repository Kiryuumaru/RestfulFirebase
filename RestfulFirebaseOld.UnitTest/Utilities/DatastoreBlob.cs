using System.Collections.Concurrent;

namespace RestfulFirebase.UnitTest.Utilities
{
    public class DatastoreBlob : ILocalDatabase
    {
        private readonly ConcurrentDictionary<string, string?> db = new();

        public DatastoreBlob()
        {

        }

        public ConcurrentDictionary<string, string?> GetDB()
        {
            return db;
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        /// <inheritdoc/>
        public string? Get(string key)
        {
            if (!db.TryGetValue(key, out string? value))
            {
                return null;
            }
            return value;
        }

        /// <inheritdoc/>
        public void Set(string key, string? value)
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
