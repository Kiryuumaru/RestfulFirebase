using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Local
{
    public class SimpleLocalDatabase : ILocalDatabase
    {
        private ConcurrentDictionary<string, string> db = new ConcurrentDictionary<string, string>();

        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        public string Get(string key)
        {
            return db.ContainsKey(key) ? db[key] : null;
        }

        public IEnumerable<string> GetKeys()
        {
            return db.Keys;
        }

        public void Set(string key, string value)
        {
            db[key] = value;
        }

        public void Delete(string key)
        {
            if (!db.TryRemove(key, out _)) db[key] = null;
        }
    }
}
