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
        bool HasRealtimeWire { get; }
        string RealtimeWirePath { get; }
        FirebaseQuery RealtimeWire { get; }
        void StartRealtime(FirebaseQuery query, bool invokeSetFirst);
        void ConsumeStream(StreamObject streamObject);
        void Delete();
    }
}
