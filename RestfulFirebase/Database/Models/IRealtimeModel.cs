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
        FirebaseQuery Query { get; }
        bool HasFirstStream { get; }
        string Key { get; }
        SmallDateTime Modified { get; }
        void StartRealtime(FirebaseQuery parent);
        void StopRealtime();
        bool ConsumeStream(StreamObject streamObject);
        void Delete();
    }
}
