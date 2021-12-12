using ObservableHelpers;
using ObservableHelpers.Abstraction;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    internal interface IInternalRealtimeModel : IRealtimeModel
    {
        bool? IsInvokeToSetFirst { get; }

        bool HasPostAttachedRealtime { get; }

        void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst);

        void DetachRealtime();
    }
}
