using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;

namespace RestfulFirebase.Database.Query
{
    public interface IFirebaseQuery
    {
        RestfulFirebaseApp App { get; }
        Task Put(string data, CancellationToken? token = null, Action<RetryExceptionEventArgs<FirebaseDatabaseException>> onException = null);
        Task Put(Func<string> data, CancellationToken? token = null, Action<RetryExceptionEventArgs<FirebaseDatabaseException>> onException = null);
        RealtimeWire<T> PutAsRealtime<T>(string key, T model) where T : IRealtimeModel;
        RealtimeWire<T> SubAsRealtime<T>(string key, T model) where T : IRealtimeModel;
        Task<string> BuildUrlAsync(CancellationToken? token = null);
        string GetAbsolutePath();
    }
}
