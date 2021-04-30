using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class BranchNode : DataNode
    {
        #region Properties

        public DataNode this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return null;
                var path = Helpers.CombineUrl(Path, key);
                return new DataNode(App, path);
            }
        }

        #endregion

        #region Initializers

        public BranchNode(RestfulFirebaseApp app, string path)
            : base(app, path)
        {

        }

        public BranchNode(DataNode dataNode)
            : base(dataNode.App, dataNode.Path)
        {

        }

        #endregion

        #region Methods

        public IEnumerable<DataNode> GetAll()
        {
            var subPaths = App.Database.OfflineDatabase.GetSubPaths(Helpers.CombineUrl(OfflineDatabase.ShortPath, Path));
            var subDatas = new List<DataNode>();
            foreach (var path in subPaths)
            {
                subDatas.Add(new DataNode(App, path));
            }
            return subDatas;
        }

        public override bool Delete()
        {
            if (!Exist) return false;
            base.Delete();
            var shortPath = Short;
            foreach (var node in GetAll())
            {
                node.Delete();
            }
            return true;
        }

        #endregion
    }
}
