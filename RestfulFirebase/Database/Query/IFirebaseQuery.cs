﻿namespace RestfulFirebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using RestfulFirebase.Database.Streaming;

    /// <summary>
    /// The FirebaseQuery interface.
    /// </summary>
    public interface IFirebaseQuery
    {
        /// <summary>
        /// Gets the owning app of this query.
        /// </summary>
        RestfulFirebaseApp App
        {
            get;
        }

        Task SetAsync(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null);

        void Set(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task SetAsync(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null);

        void Set(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task<FirebaseProperty<T>> GetAsPropertyAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task<FirebaseObject> GetAsStorableAsync(string path, TimeSpan? timeout = null, Action<Exception> onException = null);

        Task<string> BuildUrlAsync();
    }
}
