using System.Text.Json.Serialization;

namespace RestfulFirebase.Authentication.Internals;

internal class FirebaseAuth
{
    [JsonPropertyName("idToken")]
    public string? FirebaseToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expiresIn")]
    //[JsonConverter(typeof(JsonConverterNullableInt))]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("localId")]
    public string? LocalId { get; set; } = "";

    [JsonPropertyName("federatedId")]
    public string? FederatedId { get; set; } = "";

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; } = "";

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; } = "";

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; } = "";

    [JsonPropertyName("email")]
    public string? Email { get; set; } = "";

    [JsonPropertyName("emailVerified")]
    public bool IsEmailVerified { get; set; } = false;

    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; } = "";
}
