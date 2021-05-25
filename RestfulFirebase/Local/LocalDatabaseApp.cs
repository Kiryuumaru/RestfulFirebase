using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabaseApp
    {
        #region Properties

        private const string KeyHeirPath = "key";
        private const string ValuePath = "val";

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
            lock (this)
            {
                var separated = Utils.UrlSeparate(path);
                var keyHeir = KeyHeirPath;
                for (int i = 0; i < separated.Length - 1; i++)
                {
                    keyHeir = Utils.UrlCombine(keyHeir, separated[i]);
                    var heirs = db.Get(ValidatePath(keyHeir));
                    var deserialized = Utils.DeserializeString(heirs)?.ToList() ?? new List<string>();
                    if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                    var serialized = Utils.SerializeString(deserialized.ToArray());
                    db.Set(ValidatePath(keyHeir), serialized);
                }
                db.Set(path, data);
            }
        }

        public string Get(string path)
        {
            path = ValidatePath(path);
            lock (this)
            {
                return db.Get(path);
            }
        }

        public void Delete(string path)
        {
            path = ValidatePath(path);
            lock (this)
            {
                var subKeyHierPath = ValidatePath(Utils.UrlCombine(KeyHeirPath, path));
                if (db.Get(subKeyHierPath) == null)
                {
                    var separated = Utils.UrlSeparate(path);
                    for (int i = separated.Length - 1; i >= 0; i--)
                    {
                        var keyHierList = separated.Take(i + 1).ToList();
                        if (separated.Length - 1 != i)
                        {
                            var valuePath = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                            if (db.Get(valuePath) != null) break;
                        }
                        keyHierList = keyHierList.Take(i).ToList();
                        keyHierList.Insert(0, KeyHeirPath);
                        var keyHeir = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                        var hiers = db.Get(keyHeir);
                        var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                        if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                        if (deserialized.Count == 0)
                        {
                            db.Delete(keyHeir);
                        }
                        else
                        {
                            var serialized = Utils.SerializeString(deserialized.ToArray());
                            db.Set(keyHeir, serialized);
                            break;
                        }
                    }
                }
                db.Delete(path);
            }
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            List<string> subPaths = new List<string>();
            void recursive(string subPath)
            {
                var s = Utils.UrlCombine(KeyHeirPath, subPath);
                var heirs = db.Get(ValidatePath(s));
                var deserialized = Utils.DeserializeString(heirs)?.ToList() ?? new List<string>();
                if (path != subPath)
                {
                    if (db.ContainsKey(ValidatePath(subPath)))
                    {
                        subPaths.Add(subPath);
                    }
                }
                if (deserialized.Count != 0)
                {
                    foreach (var heir in deserialized)
                    {
                        recursive(Utils.UrlCombine(subPath, heir));
                    }
                }
            }
            lock (this)
            {
                recursive(path);
                return subPaths;
            }
        }

        public bool ContainsData(string path)
        {
            path = ValidatePath(path);
            lock (this)
            {
                return db.ContainsKey(path);
            }
        }

        #endregion
    }
}
