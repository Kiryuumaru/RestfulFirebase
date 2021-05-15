using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Storage
{
    public class FirebaseMetaData
    {
        [JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [JsonPropertyName("generation")]
        public string Generation { get; set; }

        [JsonPropertyName("metageneration")]
        public string MetaGeneration { get; set; }

        [JsonPropertyName("fullPath")]
        public string FullPath { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("timeCreated")]
        public DateTime TimeCreated { get; set; }

        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        [JsonPropertyName("md5Hash")]
        public string Md5Hash { get; set; }

        [JsonPropertyName("contentEncoding")]
        public string ContentEncoding { get; set; }

        [JsonPropertyName("contentDisposition")]
        public string ContentDisposition { get; set; }
    }
}
