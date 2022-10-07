using ObservableHelpers.ComponentModel;
using RestfulFirebase.Http;
using System.Net.Http;
using System.Text.Json;

namespace RestfulFirebase;

/// <summary>
/// Provides configuration for all operation.
/// </summary>
[ObservableObject]
public partial class FirebaseConfig
{
    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ProjectId { get; }

    /// <summary>
    /// Gets or sets the <see cref="IHttpClientFactory"/>.
    /// </summary>
    [ObservableProperty]
    private IHttpClientFactory? httpClientFactory;

    /// <summary>
    /// Gets or sets the default <see cref="System.Text.Json.JsonSerializerOptions"/>.
    /// </summary>
    [ObservableProperty]
    JsonSerializerOptions? jsonSerializerOptions;

    /// <summary>
    /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
    /// </summary>
    /// <param name="apiKey">
    /// The API key of the app.
    /// </param>
    /// <param name="projectId">
    /// The project ID of the app.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projectId"/> or
    /// <paramref name="projectId"/> is a null reference.
    /// </exception>
    public FirebaseConfig(string projectId, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(projectId);
        ArgumentNullException.ThrowIfNull(apiKey);

        ApiKey = apiKey;
        ProjectId = projectId;
    }
}
