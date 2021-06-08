using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IObservable, IDisposable
    {
        event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        bool HasAttachedRealtime { get; }
        void AttachRealtime(RealtimeInstance modelWire, bool invokeSetFirst);
        void DetachRealtime();
        Task<bool> WaitForSynced(TimeSpan timeout);
    }
}
