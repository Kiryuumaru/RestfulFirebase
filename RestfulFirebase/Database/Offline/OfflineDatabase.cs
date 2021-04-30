using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
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
        internal static readonly string ShortPath = Helpers.CombineUrl(Root, "short");
        internal static readonly string LongPath = Helpers.CombineUrl(Root, "long");
        internal static readonly string SyncBlobPath = Helpers.CombineUrl(Root, "blob");
        internal static readonly string ChangesPath = Helpers.CombineUrl(Root, "changes");
        internal static readonly string SyncStratPath = Helpers.CombineUrl(Root, "strat");

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

        public IEnumerable<string> GetSubPaths(string path)
        {
            return App.LocalDatabase.GetSubPaths(path);
        }

        public void Flush()
        {
            foreach (var path in App.LocalDatabase.GetSubPaths(Root))
            {
                App.LocalDatabase.Delete(path);
            }
        }

        #endregion
    }
}
