using System.ComponentModel;
using Newtonsoft.Json;

namespace RestfulFirebase.Auth
{
    public class User
    {
        [JsonProperty("localId", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string LocalId { get; set; }

        [JsonProperty("federatedId", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FederatedId { get; set; }

        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FirstName { get; set; }

        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string LastName { get; set; }

        [JsonProperty("displayName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string DisplayName { get; set; }

        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Email { get; set; }

        [JsonProperty("emailVerified", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool IsEmailVerified { get; set; }

        [JsonProperty("photoUrl", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string PhotoUrl { get; set; }

        [JsonProperty("phoneNumber", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string PhoneNumber { get; set; }
    }
}
