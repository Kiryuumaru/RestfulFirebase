using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Utilities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Authentication.Models;

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
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> whether the user is registered; otherwise <c>false</c>.
    /// </summary>
    [JsonPropertyName("registered")]
    public bool IsRegistered { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> if the <see cref="AuthUri"/> is for user's existing provider; otherwise <c>false</c>.
    /// </summary>
    [JsonPropertyName("forExistingProvider")]
    public bool IsForExistingProvider { get; set; }

    /// <summary>
    /// The URI used by the IDP to authenticate the user.
    /// </summary>
    [JsonPropertyName("authUri")]
    public string? AuthUri { get; set; }

    /// <summary>
    /// The provider ID of the auth URI.
    /// </summary>
    [JsonPropertyName("providerId")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FirebaseAuthType? ProviderId { get; set; }

    /// <summary>
    /// All provider ID of the auth URI.
    /// </summary>
    [JsonPropertyName("allProviders")]
    [JsonConverter(typeof(ItemConverterDecorator<JsonStringEnumConverter>))]
    public List<FirebaseAuthType> Providers { get; set; }
}
