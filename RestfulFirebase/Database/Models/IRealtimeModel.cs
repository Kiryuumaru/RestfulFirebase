using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IObservable, IDisposable
    {
        void DetachRealtime();
    }

    internal interface IRealtimeModelProxy : IRealtimeModel
    {
        void StartRealtime(RealtimeInstance modelWire, bool invokeSetFirst);
    }
}
