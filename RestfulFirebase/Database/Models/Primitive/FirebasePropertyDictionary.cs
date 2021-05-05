using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Models.Primitive;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebasePropertyDictionary : ObservableDictionary<string, FirebaseProperty>, IRealtimeModel
    {
        public string Key => throw new NotImplementedException();

        public bool Delete()
        {
            throw new NotImplementedException();
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            throw new NotImplementedException();
        }
    }
}
