using Newtonsoft.Json.Linq;
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
    public class DatastoreJson : ILocalDatabase
    {
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "db.db");
        private static JObject db = new JObject();
        private bool isPersistent;

        public DatastoreJson(bool isPersistent)
        {
            this.isPersistent = isPersistent;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "");
            if (isPersistent) db = new JObject(File.ReadAllText(filePath));
        }

        private void Save()
        {
            if (!isPersistent) return;
            Task.Run(delegate
            {
                string dbCopy = "";
                lock (db)
                {
                    dbCopy = db.ToString();
                }
                lock (this)
                {
                    try
                    {
                        File.WriteAllText(filePath, dbCopy);
                        Thread.Sleep(500);
                    }
                    catch { }
                }
            });
        }

        public IEnumerable<string> GetKeys()
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public string Get(string key)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Delete(string key)
        {
            throw new NotImplementedException();
        }
    }
}
