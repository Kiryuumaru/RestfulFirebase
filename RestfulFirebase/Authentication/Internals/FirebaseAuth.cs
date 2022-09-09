using System.Text.Json.Serialization;

namespace RestfulFirebase.Authentication.Internals;

internal class FirebaseAuth
{
    public string? IdToken { get; set; }

    public string? RefreshToken { get; set; }

    public int? ExpiresIn { get; set; }

    public string? LocalId { get; set; }

    public string? FederatedId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public bool IsEmailVerified { get; set; }

    public string? PhotoUrl { get; set; }

    public string? PhoneNumber { get; set; } 
}
