using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class EndNode : DataNode
    {
        #region Properties

        public string SyncBlob
        {
            get => Get(OfflineDatabase.SyncBlobPath, Short);
            set => Set(value, OfflineDatabase.SyncBlobPath, Short);
        }

        public DataChanges Changes
        {
            get => DataChanges.Parse(Get(OfflineDatabase.ChangesPath, Short));
            set => Set(value?.ToData(), OfflineDatabase.ChangesPath, Short);
        }

        public string LatestBlob
        {
            get
            {
                if (!Exist) return null;
                var sync = SyncBlob;
                var changes = Changes;
                return changes == null ? sync : changes.Blob;
            }
        }

        public SyncStrategy SyncStrategy
        {
            get
            {
                switch (Get(OfflineDatabase.SyncStratPath, Short))
                {
                    case "1":
                        return SyncStrategy.Active;
                    case "2":
                        return SyncStrategy.Passive;
                    default:
                        return SyncStrategy.None;
                }
            }
            set
            {
                switch (value)
                {
                    case SyncStrategy.Active:
                        Set("1", OfflineDatabase.SyncStratPath, Short);
                        break;
                    case SyncStrategy.Passive:
                        Set("2", OfflineDatabase.SyncStratPath, Short);
                        break;
                    default:
                        Set("0", OfflineDatabase.SyncStratPath, Short);
                        break;
                }
            }
        }

        #endregion

        #region Initializers

        public EndNode(RestfulFirebaseApp app, string path)
            : base(app, path)
        {

        }

        public EndNode(DataNode dataNode)
            : base(dataNode.App, dataNode.Path)
        {

        }

        #endregion

        #region Methods

        public override bool Delete()
        {
            if (!Exist) return false;
            base.Delete();
            var shortPath = Short;
            Set(null, OfflineDatabase.SyncBlobPath, shortPath);
            Set(null, OfflineDatabase.ChangesPath, shortPath);
            Set(null, OfflineDatabase.SyncStratPath, shortPath);
            return true;
        }

        #endregion
    }
}
