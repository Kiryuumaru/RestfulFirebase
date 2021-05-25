using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;

namespace RestfulFirebase.Database.Query
{
    public interface IFirebaseQuery
    {
        RestfulFirebaseApp App { get; }
        ChildQuery Child(Func<string> pathFactory);
        ChildQuery Child(string path);
        Task Put(string data, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);
        Task Put(Func<string> data, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);
        Task<string> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);
        T PutAsRealtime<T>(T model) where T : IRealtimeModel;
        T SubAsRealtime<T>(T model) where T : IRealtimeModel;
        RealtimeWire AsRealtimeWire();
        Task<string> BuildUrlAsync(CancellationToken? token = null);
        string GetAbsolutePath();
    }
}
