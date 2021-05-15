using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Auth
{
    public class ProviderQueryResult
    {
        internal ProviderQueryResult()
        {
            Providers = new List<FirebaseAuthType>();
        }

        public string Email { get; set; }

        [JsonPropertyName("registered")]
        public bool IsRegistered { get; set; }

        [JsonPropertyName("forExistingProvider")]
        public bool IsForExistingProvider { get; set; }

        [JsonPropertyName("authUri")]
        public string AuthUri { get; set; }

        [JsonPropertyName("providerId")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FirebaseAuthType? ProviderId { get; set; }

        [JsonPropertyName("allProviders")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public List<FirebaseAuthType> Providers { get; set; }
    }
}
