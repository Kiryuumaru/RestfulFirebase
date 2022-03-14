using ObservableHelpers;
using ObservableHelpers.Abstraction;
using RestfulFirebase.RealtimeDatabase.Models;
using RestfulFirebase.RealtimeDatabase.Realtime;
using RestfulFirebase.Exceptions;
using SynchronizationContextHelpers;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Models;

internal interface IInternalRealtimeModel : IRealtimeModel
{
    SyncOperation SyncOperation { get; }

    bool? IsInvokeToSetFirst { get; }

    bool HasPostAttachedRealtime { get; }

    void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst);

    void DetachRealtime();
}
