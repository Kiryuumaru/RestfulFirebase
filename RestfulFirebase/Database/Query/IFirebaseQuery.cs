using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Streaming;

namespace RestfulFirebase.Database.Query
{
    public interface IFirebaseQuery
    {
        RestfulFirebaseApp App { get; }
        Task Put(string data, TimeSpan? timeout = null, Action<FirebaseException> onException = null);
        RealtimeWire<T> AsRealtime<T>(T model) where T : IRealtimeModel;
        Task<string> BuildUrlAsync();
        string GetAbsolutePath();
    }
}
