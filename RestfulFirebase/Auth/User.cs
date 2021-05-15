using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Auth
{
    public class User
    {
        [JsonPropertyName("localId")]
        [DefaultValue("")]
        public string LocalId { get; set; } = "";

        [JsonPropertyName("federatedId")]
        [DefaultValue("")]
        public string FederatedId { get; set; } = "";

        [JsonPropertyName("firstName")] 
        [DefaultValue("")]
        public string FirstName { get; set; } = "";

        [JsonPropertyName("lastName")]
        [DefaultValue("")]
        public string LastName { get; set; } = "";

        [JsonPropertyName("displayName")]
        [DefaultValue("")]
        public string DisplayName { get; set; } = "";

        [JsonPropertyName("email")]
        [DefaultValue("")]
        public string Email { get; set; } = "";

        [JsonPropertyName("emailVerified")]
        [DefaultValue(false)]
        public bool IsEmailVerified { get; set; } = false;

        [JsonPropertyName("photoUrl")]
        [DefaultValue("")]
        public string PhotoUrl { get; set; } = "";

        [JsonPropertyName("phoneNumber")]
        [DefaultValue("")]
        public string PhoneNumber { get; set; } = "";
    }
}
