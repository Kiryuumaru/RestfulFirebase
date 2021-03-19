using Newtonsoft.Json;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    public class FirebaseConfig
    {
        public FirebaseConfig()
        {
            //OfflineDatabaseFactory = (t, s) => new Dictionary<string, OfflineEntry>();
            SubscriptionStreamReaderFactory = s => new StreamReader(s);
            JsonSerializerSettings = new JsonSerializerSettings();
            SyncPeriod = TimeSpan.FromSeconds(10);
            HttpClientFactory = new TransientHttpClientFactory();
        }

        public string ApiKey { get; set; }

        public string DatabaseURL { get; set; }

        public string StorageBucket { get; set; }

        public Func<Stream, TextReader> SubscriptionStreamReaderFactory { get; set; }

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public TimeSpan SyncPeriod { get; set; }

        public bool AsAccessToken { get; set; }

        public bool StorageThrowOnCancel { get; set; }

        public IHttpClientFactory HttpClientFactory { get; set; }
    }
}
