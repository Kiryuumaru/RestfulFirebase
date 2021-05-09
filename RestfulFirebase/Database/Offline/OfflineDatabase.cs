using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineDatabase : IDisposable
    {
        #region Properties

        internal const string Root = "offdb";
        internal static readonly string ShortPath = Utils.CombineUrl(Root, "short");
        internal static readonly string SyncBlobPath = Utils.CombineUrl(Root, "blob");
        internal static readonly string ChangesPath = Utils.CombineUrl(Root, "changes");

        public RestfulFirebaseApp App { get; }

        #endregion

        #region Initializers

        public OfflineDatabase(RestfulFirebaseApp app)
        {
            App = app;
        }

        #endregion

        #region Methods

        public DataNode GetData(string path)
        {
            return new DataNode(App, path);
        }

        public IEnumerable<DataNode> GetSubDatas(string path)
        {
            var datas = new List<DataNode>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath, path)))
            {
                datas.Add(new DataNode(App, subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public IEnumerable<DataNode> GetAllDatas()
        {
            var datas = new List<DataNode>();
            foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath)))
            {
                datas.Add(new DataNode(App, subPath.Substring(ShortPath.Length)));
            }
            return datas;
        }

        public DataNode GetFirstSyncPriority()
        {
            DataNode first = null;
            DataChanges firstChanges = null;
            foreach (var node in GetAllDatas())
            {
                var nodeChanges = node?.Changes;
                if (nodeChanges != null)
                {
                    if (firstChanges == null || firstChanges?.SyncPriority <= nodeChanges.SyncPriority)
                    {
                        first = node;
                        firstChanges = node.Changes;
                    }
                }
            }
            return first;
        }

        public DataNode GetLastSyncPriority()
        {
            DataNode last = null;
            DataChanges lastChanges = null;
            foreach (var node in GetAllDatas())
            {
                var nodeChanges = node?.Changes;
                if (nodeChanges != null)
                {
                    if (lastChanges == null || lastChanges?.SyncPriority >= nodeChanges.SyncPriority)
                    {
                        last = node;
                        lastChanges = node.Changes;
                    }
                }
            }
            return last;
        }

        public long GetAvailableSyncPriority()
        {
            var lastPriority = App.Database.OfflineDatabase.GetLastSyncPriority();
            return lastPriority?.Changes == null ? 0 : lastPriority.Changes.SyncPriority + 1;
        }

        public void Flush()
        {
            var subPaths = App.LocalDatabase.GetSubPaths(Root);
            foreach (var subPath in subPaths)
            {
                App.LocalDatabase.Delete(subPath);
            }
        }

        public void Dispose()
        {

        }

        #endregion
    }
}
