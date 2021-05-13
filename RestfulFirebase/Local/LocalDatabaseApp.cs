using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabaseApp
    {
        #region Properties

        private const string KeyHeirPath = "key";

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
                var separated = Utils.SeparateUrl(path);
                var keyHeir = KeyHeirPath;
                for (int i = 0; i < separated.Length - 1; i++)
                {
                    keyHeir = ValidatePath(Utils.CombineUrl(keyHeir, separated[i]));
                    var heirs = db.Get(keyHeir);
                    var deserialized = Utils.DeserializeString(heirs)?.ToList() ?? new List<string>();
                    if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                    var serialized = Utils.SerializeString(deserialized.ToArray());
                    db.Set(keyHeir, serialized);
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
                var separated = Utils.SeparateUrl(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHeirList = separated.Take(i).ToList();
                    keyHeirList.Insert(0, KeyHeirPath);
                    var keyHeir = ValidatePath(Utils.CombineUrl(keyHeirList.ToArray()));
                    var heirs = db.Get(keyHeir);
                    var deserialized = Utils.DeserializeString(heirs)?.ToList() ?? new List<string>();
                    if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                    if (deserialized.Count == 0)
                    {
                        db.Delete(keyHeir);
                    }
                    else
                    {
                        var serialized = Utils.SerializeString(deserialized.ToArray());
                        db.Set(keyHeir, serialized);
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
                var s = Utils.CombineUrl(KeyHeirPath, subPath);
                var heirs = db.Get(ValidatePath(s));
                var deserialized = Utils.DeserializeString(heirs)?.ToList() ?? new List<string>();
                if (deserialized.Count == 0)
                {
                    if (path != subPath) subPaths.Add(subPath);
                }
                else
                {
                    foreach (var heir in deserialized)
                    {
                        recursive(Utils.CombineUrl(subPath, heir));
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
