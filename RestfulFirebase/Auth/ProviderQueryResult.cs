using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RestfulFirebase.Auth
{
    /// <summary>
    /// More info at <see href="https://developers.google.com/identity/toolkit/web/reference/relyingparty/createAuthUri"/>.
    /// </summary>
    public class ProviderQueryResult
    {
        internal ProviderQueryResult()
        {
            Providers = new List<FirebaseAuthType>();
        }

        /// <summary>
        /// The underlying email of the auth provider.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets <c>true</c> whether the user is registered; otherwise <c>false</c>.
        /// </summary>
        [JsonProperty("registered")]
        public bool IsRegistered { get; set; }

        /// <summary>
        /// Gets or sets <c>true</c> if the <see cref="AuthUri"/> is for user's existing provider; otherwise <c>false</c>.
        /// </summary>
        [JsonProperty("forExistingProvider")]
        public bool IsForExistingProvider { get; set; }

        /// <summary>
        /// The URI used by the IDP to authenticate the user.
        /// </summary>
        [JsonProperty("authUri")]
        public string AuthUri { get; set; }

        /// <summary>
        /// The provider ID of the auth URI.
        /// </summary>
        [JsonProperty("providerId")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FirebaseAuthType? ProviderId { get; set; }

        /// <summary>
        /// All provider ID of the auth URI.
        /// </summary>
        [JsonProperty("allProviders", ItemConverterType = typeof(StringEnumConverter))]
        public List<FirebaseAuthType> Providers { get; set; }
    }
}
