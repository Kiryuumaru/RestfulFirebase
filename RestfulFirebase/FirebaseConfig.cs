using Newtonsoft.Json;
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
            SyncPeriod = TimeSpan.FromSeconds(10);
            HttpClientFactory = new TransientHttpClientFactory();
        }

        public string ApiKey { get; set; }

        public string DatabaseURL { get; set; }

        public string StorageBucket { get; set; }

        public ILocalDatabase LocalDatabase { get; set; }

        public TimeSpan SyncPeriod { get; set; }

        public bool AsAccessToken { get; set; }

        public bool StorageThrowOnCancel { get; set; }

        public IHttpClientFactory HttpClientFactory { get; set; }
    }
}
