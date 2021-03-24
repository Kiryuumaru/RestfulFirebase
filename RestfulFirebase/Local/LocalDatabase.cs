using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabase
    {
        public RestfulFirebaseApp App { get; }

        private IDictionary<string, string> db => App.Config.LocalDatabase;

        public LocalDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        public void Set(string path, string data)
        {
            path = ValidatePath(path);
            db[path] = data;
        }

        public string Get(string path)
        {
            path = ValidatePath(path);
            return db.ContainsKey(path) ? db[path] : null;
        }

        public void Delete(string path)
        {
            path = ValidatePath(path);
            db.Remove(path);
        }

        public void DeletePath(string path)
        {
            path = ValidatePath(path);
            foreach (var subPath in db.Keys.Where(i => i.StartsWith(path)))
            {
                db.Remove(subPath);
            }
            db.Remove(path);
        }

        public IEnumerable<string> GetAll(string path)
        {
            path = ValidatePath(path);
            var paths = db.Keys.Where(i => i.StartsWith(path));
            var ret = new List<string>();
            foreach (var subPath in paths)
            {
                if (db.ContainsKey(path)) ret.Add(db[subPath]);
            }
            return ret;
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            path = ValidatePath(path);
            return db.Keys.Where(i => i.StartsWith(path));
        }

        public bool ContainsPath(string path)
        {
            path = ValidatePath(path);
            return db.Keys.Where(i => i.StartsWith(path) && !i.Equals(path)).Count() != 0;
        }

        public bool ContainsData(string path)
        {
            path = ValidatePath(path);
            return db.ContainsKey(path);
        }

        private string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is null or empty");
            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }
    }
}
