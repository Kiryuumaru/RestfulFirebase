using System.ComponentModel;
using Newtonsoft.Json;

namespace RestfulFirebase.Auth
{
    /// <summary>
    /// Provides raw firebase user JSON properties.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the local id of the user.
        /// </summary>
        [JsonProperty("localId", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string LocalId { get; set; }

        /// <summary>
        /// Gets or sets the federated id of the user.
        /// </summary>
        [JsonProperty("federatedId", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FederatedId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        [JsonProperty("displayName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email of the user.
        /// </summary>
        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the email verfication status of the user.
        /// </summary>
        [JsonProperty("emailVerified", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the photo url of the user.
        /// </summary>
        [JsonProperty("photoUrl", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [JsonProperty("phoneNumber", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string PhoneNumber { get; set; }
    }
}
