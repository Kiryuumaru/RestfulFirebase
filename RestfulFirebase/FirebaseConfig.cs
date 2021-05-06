using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Extensions.Http;
using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    public class FirebaseConfig
    {
        public FirebaseConfig()
        {
            LocalDatabase = new SimpleLocalDatabase();
            HttpClientFactory = new TransientHttpClientFactory();
            AuthRequestTimeout = TimeSpan.FromMinutes(1);
            DatabaseRequestTimeout = TimeSpan.FromMinutes(2);
            StorageRequestTimeout = TimeSpan.FromMinutes(2);
        }

        public string ApiKey { get; set; }

        public string DatabaseURL { get; set; }

        public string StorageBucket { get; set; }

        public TimeSpan AuthRequestTimeout { get; set; }

        public TimeSpan DatabaseRequestTimeout { get; set; }

        public TimeSpan StorageRequestTimeout { get; set; }

        public ILocalDatabase LocalDatabase { get; set; }

        public bool AsAccessToken { get; set; }

        public bool StorageThrowOnCancel { get; set; }

        public bool OfflineMode { get; set; }

        public IHttpClientFactory HttpClientFactory { get; set; }
    }
}
