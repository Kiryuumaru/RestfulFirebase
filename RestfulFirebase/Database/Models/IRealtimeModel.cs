using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel
    {
        bool HasRealtimeWire { get; }
        string RealtimeWirePath { get; }
        void OnError(Exception exception, bool defaultIgnoreAndContinue = true);
        void OnError(ContinueExceptionEventArgs args);
        void SetRealtime(IFirebaseQuery query, bool invokeSetFirst, out Action<StreamEvent> onNext);
        void Delete();
    }
}
