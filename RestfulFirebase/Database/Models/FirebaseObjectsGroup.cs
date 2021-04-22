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

        public RealtimeWire RealtimeWire => throw new NotImplementedException();

        public FirebaseQuery Query => throw new NotImplementedException();

        public bool HasFirstStream => throw new NotImplementedException();

        #endregion

        #region Initializers

        public FirebaseObjectsGroup(string key) : base()
        {
            Key = key;
        }

        public void StartRealtime(FirebaseQuery query)
        {
            throw new NotImplementedException();
        }

        public void StopRealtime()
        {
            throw new NotImplementedException();
        }

        public bool ConsumeStream(StreamObject streamObject)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods


        #endregion
    }
}
