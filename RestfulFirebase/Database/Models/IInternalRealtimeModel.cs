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
        void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst);

        Task AttachRealtimeAsync(RealtimeInstance realtimeInstance, bool invokeSetFirst);

        void DetachRealtime();
    }
}
