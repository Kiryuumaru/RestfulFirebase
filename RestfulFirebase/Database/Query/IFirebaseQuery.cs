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

        RealtimeHolder<FirebaseProperty> SetStream(FirebaseProperty property);

        RealtimeHolder<FirebaseObject> SetStream(FirebaseObject obj);

        RealtimeHolder<FirebaseProperty> GetStreamAsProperty(string path);

        RealtimeHolder<FirebaseObject> GetStreamAsObject(string path);

        RealtimeHolder<FirebasePropertyGroup> GetStreamAsPropertyCollection(string path);

        RealtimeHolder<FirebaseObjectGroup> GetStreamAsObjectCollection(string path);

        void Delete(string path);

        Task<string> BuildUrlAsync();

        string GetAbsolutePath();
    }
}
