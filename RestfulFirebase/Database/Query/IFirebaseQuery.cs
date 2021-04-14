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
        void Put(string data, TimeSpan? timeout = null, Action<FirebaseException> onException = null);
        RealtimeHolder<FirebaseProperty<T>> AsRealtimeProperty<T>(FirebaseProperty<T> property);
        RealtimeHolder<FirebaseProperty<T>> AsRealtimeProperty<T>(string path);
        RealtimeHolder<FirebaseProperty> AsRealtimeProperty(FirebaseProperty property);
        RealtimeHolder<FirebaseProperty> AsRealtimeProperty(string path);
        RealtimeHolder<T> AsRealtimeObject<T>(T obj) where T : FirebaseObject;
        RealtimeHolder<T> AsRealtimeObject<T>(string path) where T : FirebaseObject;
        RealtimeHolder<FirebaseObject> AsRealtimeObject(FirebaseObject obj);
        RealtimeHolder<FirebaseObject> AsRealtimeObject(string path);
        RealtimeHolder<FirebasePropertyGroup> AsRealtimePropertyGroup(FirebasePropertyGroup group);
        RealtimeHolder<FirebasePropertyGroup> AsRealtimePropertyGroup(string path);
        RealtimeHolder<FirebaseObjectGroup> AsRealtimeObjectGroup(FirebaseObjectGroup group);
        RealtimeHolder<FirebaseObjectGroup> AsRealtimeObjectGroup(string path);
        Task<string> BuildUrlAsync();
        string GetAbsolutePath();
    }
}
