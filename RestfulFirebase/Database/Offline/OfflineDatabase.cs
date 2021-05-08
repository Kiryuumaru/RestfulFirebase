using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineDatabase
    {
        #region Properties

        internal const string Root = "offdb";
        internal static readonly string ShortPath = Utils.CombineUrl(Root, "short");
        internal static readonly string SyncBlobPath = Utils.CombineUrl(Root, "blob");
        internal static readonly string ChangesPath = Utils.CombineUrl(Root, "changes");

        public RestfulFirebaseApp App { get; }

        public event Action<DataChanges> OnChanges;

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

        public void Flush(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                foreach (var subPath in App.LocalDatabase.GetSubPaths(Root))
                {
                    App.LocalDatabase.Delete(subPath);
                }
            }
            else
            {
                foreach (var subPath in App.LocalDatabase.GetSubPaths(Utils.CombineUrl(ShortPath, path)))
                {
                    App.LocalDatabase.Delete(subPath); 
                }
            }
        }

        #endregion
    }
}
