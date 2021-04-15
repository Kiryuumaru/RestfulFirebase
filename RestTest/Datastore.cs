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
    public class Datastore : ILocalDatabase
    {
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "db.db");
        private static Dictionary<string, string> db;

        public Datastore()
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "");
            db = Helpers.BlobConvert(File.ReadAllText(filePath));
        }

        private void Save()
        {
            Task.Run(delegate
            {
                lock (this)
                {
                    try
                    {
                        string contentCopy = Helpers.BlobConvert(new Dictionary<string, string>(db));
                        File.WriteAllText(filePath, contentCopy);
                        Thread.Sleep(500);
                    }
                    catch { }
                }
            });
        }

        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        public IEnumerable<string> GetKeys()
        {
            return db.Keys;
        }

        public string Get(string key)
        {
            try
            {
                return db[key];
            }
            catch
            {
                return null;
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
    }
}
