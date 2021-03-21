using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Streaming;

namespace RestfulFirebase.Database.Query
{
    public interface IFirebaseQuery
    {
        RestfulFirebaseApp App { get; }

        Task SetAsync(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null);

        void Set(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task SetAsync(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null);

        void Set(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task<FirebaseProperty<T>> GetAsPropertyAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task<T> GetAsObjectAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null) where T : FirebaseObject;

        Task<string> BuildUrlAsync();

        string GetAbsolutePath();
    }
}
