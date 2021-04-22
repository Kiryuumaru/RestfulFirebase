using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectsGroup : ObservableGroup<ObservableObjects>, IRealtimeModel
    {
        #region Properties

        public string Key { get; protected set; }

        public SmallDateTime Modified => throw new NotImplementedException();

        #endregion

        #region Initializers

        public FirebaseObjectsGroup(string key) : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        public void MakeRealtime(RealtimeWire wire)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
