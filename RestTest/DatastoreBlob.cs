using RestfulFirebase.Local;
using System;
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
        private static Dictionary<string, string> db;
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
            if (isPersistent) db = Utils.BlobConvert(File.ReadAllText(filePath));
            else db = new Dictionary<string, string>();
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

        public bool ContainsKey(string key)
        {
            lock (db)
            {
                return db.ContainsKey(key);
            }
        }

        public string Get(string key)
        {
            lock (db)
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
        }

        public void Set(string key, string value)
        {
            lock (db)
            {
                if (db.ContainsKey(key)) db[key] = value;
                else db.Add(key, value);
            }
            Save();
        }

        public void Delete(string key)
        {
            lock (db)
            {
                db.Remove(key);
            }
            Save();
        }

        public void Clear()
        {
            lock (db)
            {
                db.Clear();
            }
            Save();
        }
    }
}
