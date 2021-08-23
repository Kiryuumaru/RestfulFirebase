using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestTest
{
    public class DatastoreBlob : ILocalDatabase
    {
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "db.db");
        private static ConcurrentDictionary<string, string> db;
        private bool isPersistent;
        private bool isWriting;
        private bool write;

        public DatastoreBlob(bool isPersistent)
        {
            this.isPersistent = isPersistent;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "");
            if (isPersistent) db = new ConcurrentDictionary<string, string>(Utils.BlobConvert(File.ReadAllText(filePath)));
            else db = new ConcurrentDictionary<string, string>();
        }

        private void Save()
        {
            if (!isPersistent) return;

            Task.Run(delegate
            {
                write = true;
                if (isWriting) return;
                isWriting = true;
                while (write)
                {
                    write = false;
                    try
                    {
                        Dictionary<string, string> dbCopy = null;
                        lock (db)
                        {
                            dbCopy = new Dictionary<string, string>(db);
                        }
                        lock (this)
                        {
                            string contentCopy = Utils.BlobConvert(dbCopy);
                            File.WriteAllText(filePath, contentCopy);
                        }
                    }
                    catch { }
                }
                isWriting = false;
            });
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
            Save();
        }

        /// <inheritdoc/>
        public void Delete(string key)
        {
            db.TryRemove(key, out _);
            Save();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            db.Clear();
            Save();
        }
    }
}
