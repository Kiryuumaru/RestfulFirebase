using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabaseApp
    {
        #region Properties

        private ILocalDatabase db => App.Config.LocalDatabase;

        public RestfulFirebaseApp App { get; }

        #endregion

        #region Initializers

        public LocalDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        #endregion

        #region Methods

        private string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is null or empty");
            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }

        public void Set(string path, string data)
        {
            path = ValidatePath(path);
            db.Set(path, data);
        }

        public string Get(string path)
        {
            path = ValidatePath(path);
            return db.Get(path);
        }

        public void Delete(string path)
        {
            path = ValidatePath(path);
            db.Delete(path);
        }

        public IEnumerable<string> GetAll(string path)
        {
            path = ValidatePath(path);
            var paths = db.GetKeys().Where(i => i.StartsWith(path));
            var ret = new List<string>();
            foreach (var subPath in paths)
            {
                if (db.ContainsKey(path)) ret.Add(db.Get(subPath));
            }
            return ret;
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            path = ValidatePath(path);
            return db.GetKeys().Where(i => i.StartsWith(path));
        }

        public bool ContainsPath(string path)
        {
            path = ValidatePath(path);
            return db.GetKeys().Where(i => i.StartsWith(path)).Count() != 0;
        }

        public bool ContainsData(string path)
        {
            path = ValidatePath(path);
            return db.ContainsKey(path);
        }

        #endregion
    }
}
