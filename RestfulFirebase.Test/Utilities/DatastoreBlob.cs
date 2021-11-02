using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Test.Utilities
{
    public class DatastoreBlob : ILocalDatabase
    {
        private ConcurrentDictionary<string, string> db;
        private bool isWriting;
        private bool write;

        public DatastoreBlob()
        {
            db = new ConcurrentDictionary<string, string>();
        }

        public ConcurrentDictionary<string, string> GetDB()
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
