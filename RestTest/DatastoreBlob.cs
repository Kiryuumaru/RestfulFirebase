using RestfulFirebase.Common;
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

        public DatastoreBlob(bool isPersistent)
        {
            this.isPersistent = isPersistent;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "");
            if (isPersistent) db = Helpers.BlobConvert(File.ReadAllText(filePath));
            else db = new Dictionary<string, string>();
        }

        private void Save()
        {
            if (!isPersistent) return;
            Task.Run(delegate
            {
                var dbCopy = new Dictionary<string, string>();
                dbCopy = new Dictionary<string, string>(db);
                lock (this)
                {
                    try
                    {
                        string contentCopy = Helpers.BlobConvert(dbCopy);
                        File.WriteAllText(filePath, contentCopy);
                    }
                    catch { }
                }
            });
        }

        public IEnumerable<string> GetKeys()
        {
            return db.Keys;
        }

        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

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

        public void Set(string key, string value)
        {
            if (db.ContainsKey(key)) db[key] = value;
            else db.Add(key, value);
            Save();
        }

        public void Delete(string key)
        {
            db.Remove(key);
            Save();
        }
    }
}
