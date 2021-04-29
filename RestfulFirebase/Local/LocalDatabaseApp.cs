﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabaseApp
    {
        public RestfulFirebaseApp App { get; }

        private ILocalDatabase db => App.Config.LocalDatabase;

        public LocalDatabaseApp(RestfulFirebaseApp app)
        {
            App = app;
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
            var paths = db.KeysStartsWith(path);
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
            return db.KeysStartsWith(path);
        }

        public bool ContainsPath(string path)
        {
            path = ValidatePath(path);
            return db.KeysStartsWith(path).Count() != 0;
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
