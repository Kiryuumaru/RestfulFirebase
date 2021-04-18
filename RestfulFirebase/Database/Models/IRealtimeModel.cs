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
        string Key { get; }
        SmallDateTime Modified { get; }
        void StartRealtime(FirebaseQuery query);
        void StopRealtime();
        void ConsumeStream(StreamObject streamObject);
        void Delete();
    }
}
