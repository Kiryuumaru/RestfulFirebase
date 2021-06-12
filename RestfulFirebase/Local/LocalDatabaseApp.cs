﻿using RestfulFirebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Local
{
    public class LocalDatabaseApp
    {
        #region Properties

        private const string EndNodeClassifier = "0";
        private const string BranchNodeClassifier = "1";
        private const string KeyHeirPath = "key";
        private const string ValuePath = "val";

        private ConcurrentDictionary<string, string> cacheDb = new ConcurrentDictionary<string, string>();

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

        private void DBSet(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");

            lock (App.Config.LocalDatabase)
            {
                App.Config.LocalDatabase.Set(key, value);
            }
        }

        private string DBGet(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");
            lock (App.Config.LocalDatabase)
            {
                return App.Config.LocalDatabase.Get(key);
            }
        }

        private bool DBContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");
            lock (App.Config.LocalDatabase)
            {
                return App.Config.LocalDatabase.ContainsKey(key);
            }
        }

        private void DBDelete(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Key is null or empty");
            lock (App.Config.LocalDatabase)
            {
                App.Config.LocalDatabase.Delete(key);
            }
        }

        private void DBClear()
        {
            lock (App.Config.LocalDatabase)
            {
                App.Config.LocalDatabase.Clear();
            }
        }

        public void Set(string path, string data)
        {
            path = ValidatePath(path);
            var separated = Utils.UrlSeparate(path);
            var keyHeir = KeyHeirPath;
            for (int i = 0; i < separated.Length - 1; i++)
            {
                keyHeir = Utils.UrlCombine(keyHeir, separated[i]);
                var hiers = DBGet(ValidatePath(keyHeir));
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (!deserialized.Contains(separated[i + 1])) deserialized.Add(separated[i + 1]);
                var serialized = Utils.SerializeString(deserialized.ToArray());
                DBSet(ValidatePath(keyHeir), serialized);
            }
            DBSet(path, data);
        }

        public string Get(string path)
        {
            path = ValidatePath(path);
            return DBGet(path);
        }

        public void Delete(string path)
        {
            path = ValidatePath(path);
            var subKeyHierPath = ValidatePath(Utils.UrlCombine(KeyHeirPath, path));
            if (DBGet(subKeyHierPath) == null)
            {
                var separated = Utils.UrlSeparate(path);
                for (int i = separated.Length - 1; i >= 0; i--)
                {
                    var keyHierList = separated.Take(i + 1).ToList();
                    if (separated.Length - 1 != i)
                    {
                        var valuePath = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                        if (DBGet(valuePath) != null) break;
                    }
                    keyHierList = keyHierList.Take(i).ToList();
                    keyHierList.Insert(0, KeyHeirPath);
                    var keyHeir = ValidatePath(Utils.UrlCombine(keyHierList.ToArray()));
                    var hiers = DBGet(keyHeir);
                    var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                    if (deserialized.Contains(separated[i])) deserialized.Remove(separated[i]);
                    if (deserialized.Count == 0)
                    {
                        DBDelete(keyHeir);
                    }
                    else
                    {
                        var serialized = Utils.SerializeString(deserialized.ToArray());
                        DBSet(keyHeir, serialized);
                        break;
                    }
                }
            }
            DBDelete(path);
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            List<string> subPaths = new List<string>();
            void recursive(string subPath)
            {
                var s = Utils.UrlCombine(KeyHeirPath, subPath);
                var hiers = DBGet(ValidatePath(s));
                var deserialized = Utils.DeserializeString(hiers)?.ToList() ?? new List<string>();
                if (path != subPath)
                {
                    if (DBContainsKey(ValidatePath(subPath)))
                    {
                        subPaths.Add(subPath);
                    }
                }
                if (deserialized.Count != 0)
                {
                    foreach (var hier in deserialized)
                    {
                        recursive(Utils.UrlCombine(subPath, hier));
                    }
                }
            }
            recursive(path);
            return subPaths;
        }

        public bool ContainsData(string path)
        {
            path = ValidatePath(path);
            return DBContainsKey(path);
        }

        #endregion
    }
}
