using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : INotifyPropertyChanged, IDisposable
    {
        RealtimeInstance RealtimeInstance { get; }
        bool HasAttachedRealtime { get; }

        event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        event EventHandler<WireErrorEventArgs> WireError;

        void AttachRealtime(RealtimeInstance modelWire, bool invokeSetFirst);
        void DetachRealtime();

        bool SetNull();
        bool IsNull();
    }
}
