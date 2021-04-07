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

        void Set(FirebaseProperty property);

        void Set(FirebaseObject obj);

        FirebaseProperty GetAsProperty(string path);

        FirebaseObject GetAsObject(string path);

        FirebasePropertyGroup GetAsPropertyCollection(string path);

        FirebaseObjectGroup GetAsObjectCollection(string path);

        void Delete(string path);

        Task<string> BuildUrlAsync();

        string GetAbsolutePath();
    }
}
