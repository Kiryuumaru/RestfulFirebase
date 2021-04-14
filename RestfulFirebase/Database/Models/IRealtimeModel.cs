using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IObservableAttributed
    {
        RealtimeWire RealtimeWire { get; }
        void BuildRealtimeWire(FirebaseQuery query, bool invokeSetFirst);
        void Delete();
    }
}
