using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectGroup : ObservableGroup<ObservableObject>, IRealtimeModel
    {
        #region Properties

        public string Key
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        public SmallDateTime Modified => throw new NotImplementedException();

        #endregion

        #region Initializers

        public FirebaseObjectGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObjectGroup(string key)
            : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        public void MakeRealtime(RealtimeWire wire)
        {
            throw new NotImplementedException();
        }

        public bool Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
