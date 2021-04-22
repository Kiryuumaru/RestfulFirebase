using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IObservable
    {
        RealtimeWire RealtimeWire { get; }
        string Key { get; }
        SmallDateTime Modified { get; }
        void BuildRealtimeWire(FirebaseQuery parent);
        void Delete();
    }
}
