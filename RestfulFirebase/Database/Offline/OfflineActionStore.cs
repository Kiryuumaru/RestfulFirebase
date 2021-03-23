using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineActionStore
    {
        public RestfulFirebaseApp App { get; }

        private ConcurrentDictionary<string, OfflineAction> db => App.Config.OfflineActions;

        public OfflineActionStore(RestfulFirebaseApp app)
        {
            App = app;
        }

        public void SetAction(string path, OfflineAction data)
        {
            path = ValidatePath(path);
            db[path] = data;
        }

        public OfflineAction GetAction(string path)
        {
            path = ValidatePath(path);
            return db[path];
        }

        public IEnumerable<OfflineAction> GetActions(string path)
        {
            path = ValidatePath(path);
            var keys = db.Keys.Where(i => i.StartsWith(path));
            foreach (var key in keys)
            {
                yield return db[key];
            }
        }

        public IEnumerable<string> GetSubPaths(string path)
        {
            path = ValidatePath(path);
            return db.Keys.Where(i => i.StartsWith(path));
        }

        private string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Path is null or empty");
            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }
    }
}
