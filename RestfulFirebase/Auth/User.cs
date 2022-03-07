using System.Text.Json.Serialization;

namespace RestfulFirebase.Auth;

/// <summary>
/// Provides raw firebase user JSON properties.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the local id of the user.
    /// </summary>
    [JsonPropertyName("localId")]
    public string? LocalId { get; set; } = "";

    /// <summary>
    /// Gets or sets the federated id of the user.
    /// </summary>
    [JsonPropertyName("federatedId")]
    public string? FederatedId { get; set; } = "";

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; } = "";

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; } = "";

    /// <summary>
    /// Gets or sets the display name of the user.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; } = "";

    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; } = "";

    /// <summary>
    /// Gets or sets the email verfication status of the user.
    /// </summary>
    [JsonPropertyName("emailVerified")]
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets the photo url of the user.
    /// </summary>
    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; } = "";
}
