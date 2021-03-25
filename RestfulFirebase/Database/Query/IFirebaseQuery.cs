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

        Task Put(string data, TimeSpan? timeout = null, Action<Exception> onException = null);

        void Set(FirebaseProperty property, TimeSpan? timeout = null);

        void Set(FirebaseObject obj, TimeSpan? timeout = null);

        FirebaseProperty GetAsProperty(string path, TimeSpan? timeout = null);

        FirebaseObject GetAsObject(string path, TimeSpan? timeout = null);

        FirebasePropertyGroup GetAsPropertyCollection(string path, TimeSpan? timeout = null);

        FirebaseObjectGroup GetAsObjectCollection(string path, TimeSpan? timeout = null);

        Task<string> BuildUrlAsync();

        string GetAbsolutePath();
    }
}
