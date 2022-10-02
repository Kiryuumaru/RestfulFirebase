namespace RestfulFirebase;

/// <summary>
/// Provides configuration for all operation.
/// </summary>
public class FirebaseConfig
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
    /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
    /// </summary>
    /// <param name="apiKey">
    /// The API key of the app.
    /// </param>
    /// <param name="projectId">
    /// The project ID of the app.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projectId"/> and
    /// <paramref name="projectId"/> are either a null reference.
    /// </exception>
    public FirebaseConfig(string projectId, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(projectId);
        ArgumentNullException.ThrowIfNull(apiKey);

        ApiKey = apiKey;
        ProjectId = projectId;
    }
}
