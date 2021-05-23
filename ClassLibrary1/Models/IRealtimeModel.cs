using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IObservable
    {
        RealtimeWire Wire { get; }
        void MakeRealtime(RealtimeWire wire);
        void Start();
        void Stop();
        bool Delete();
    }
}
