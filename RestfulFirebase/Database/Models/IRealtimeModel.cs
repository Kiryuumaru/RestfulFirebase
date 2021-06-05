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
        event Action OnRealtimeAttached;
        event Action OnRealtimeAttachedInternal;
        event Action OnRealtimeDetached;
        event Action OnRealtimeDetachedInternal;
        bool HasAttachedRealtime { get; }
        void AttachRealtime(RealtimeInstance modelWire, bool invokeSetFirst);
        void DetachRealtime();
    }
}
