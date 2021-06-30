﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Local
{
    /// <inheritdoc/>
    public class DefaultLocalDatabase : ILocalDatabase
    {
        private static Dictionary<string, string> db = new Dictionary<string, string>();

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        /// <inheritdoc/>
        public string Get(string key)
        {
            try
            {
                if (!db.ContainsKey(key)) return null;
                return db[key];
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void Set(string key, string value)
        {
            lock (db)
            {
                if (db.ContainsKey(key)) db[key] = value;
                else db.Add(key, value);
            }
        }

        /// <inheritdoc/>
        public void Delete(string key)
        {
            lock (db)
            {
                db.Remove(key);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (db)
            {
                db.Clear();
            }
        }
    }
}