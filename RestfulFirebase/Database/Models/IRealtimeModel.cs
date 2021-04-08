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
        FirebaseQuery RealtimeWire { get; }
        void OnError(Exception exception, bool defaultIgnoreAndContinue = true);
        void OnError(ContinueExceptionEventArgs args);
        void StartRealtime(FirebaseQuery query, bool invokeSetFirst, out Action<StreamObject> onNext);
        void Delete();
    }
}
