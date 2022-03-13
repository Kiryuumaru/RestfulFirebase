using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using RestfulFirebase.Exceptions;
using System.Text.Json;

namespace RestfulFirebase.Storage;

/// <summary>
/// Provides firebase storage reference node implementations.
/// </summary>
public class FirebaseStorageReference
{
    #region Properties

    private const string FirebaseStorageEndpoint = "https://firebasestorage.googleapis.com/v0/b/";

    private readonly List<string> children;

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> this reference uses.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    #endregion

    #region Initializers

    internal FirebaseStorageReference(RestfulFirebaseApp app, string childRoot)
    {
        App = app;

        children = new List<string>
        {
            childRoot
        };
    }

    internal FirebaseStorageReference(RestfulFirebaseApp app, IEnumerable<string> children)
    {
        App = app;

        this.children = children.ToList();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Starts uploading given stream to target location.
    /// </summary>
    /// <param name="stream">
    /// Stream to upload.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token which can be used to cancel the operation.
    /// </param>
    /// <param name="mimeType">
    /// Optional type of data being uploaded, will be used to set HTTP Content-Type header.
    /// </param>
    /// <returns>
    /// The <see cref="FirebaseStorageTask"/> which can be used to track the progress of the upload.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <see cref="FirebaseConfig.StorageBucket"/> is null.
    /// </exception>
    public FirebaseStorageTask Put(Stream stream, CancellationToken cancellationToken, string? mimeType = null)
    {
        if (App.Config.CachedStorageBucket == null)
        {
            throw new StorageBucketMissingException();
        }

        return new FirebaseStorageTask(App, GetTargetUrl(), GetFullDownloadUrl(), stream, cancellationToken, mimeType);
    }

    /// <summary>
    /// Starts uploading given stream to target location.
    /// </summary>
    /// <param name="fileStream">
    /// Stream to upload.
    /// </param>
    /// <returns>
    /// The <see cref="FirebaseStorageTask"/> which can be used to track the progress of the upload.
    /// </returns>
    public FirebaseStorageTask Put(Stream fileStream)
    {
        return Put(fileStream, CancellationToken.None);
    }

    /// <summary>
    /// Gets the meta data for given file.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> which results a <see cref="FirebaseMetaData"/> of this reference.
    /// </returns>
    public async Task<FirebaseMetaData?> GetMetaData(TimeSpan? timeout = null)
    {
        var data = await PerformFetch<FirebaseMetaData>(timeout).ConfigureAwait(false);

        return data;
    }

    /// <summary>
    /// Gets the meta data for given file.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> which results a <see cref="string"/> that represents the download url of the reference.
    /// </returns>
    public async Task<string> GetDownloadUrl(TimeSpan? timeout = null)
    {
        var data = await PerformFetch<Dictionary<string, object>>(timeout).ConfigureAwait(false);

        if (data == null || !data.TryGetValue("downloadTokens", out object? downloadTokens))
        {
            throw new ArgumentOutOfRangeException($"Could not extract 'downloadTokens' property from response. Response: {JsonSerializer.Serialize(data)}");
        }

        return GetFullDownloadUrl() + downloadTokens;
    }

    /// <summary>
    /// Deletes the file of this reference.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the task returned by deletion.
    /// </returns>
    public async Task Delete(TimeSpan? timeout = null)
    {
        var url = GetDownloadUrl();
        string resultContent;

        try
        {
            using var http = App.Storage.CreateHttpClientAsync(timeout);
            var result = await http.DeleteAsync(url).ConfigureAwait(false);

            resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            result.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new StorageUndefinedException(ex);
        }
    }

    /// <summary>
    /// Creates new instance of <see cref="FirebaseStorageReference"/> child reference to the file.
    /// </summary>
    /// <param name="name">
    /// The child reference name or file name.
    /// </param>
    /// <returns>
    /// The instance of <see cref="FirebaseStorageReference"/> child reference.
    /// </returns>
    public FirebaseStorageReference Child(string name)
    {
        var children = new List<string>(this.children)
        {
            name
        };
        return new FirebaseStorageReference(App, children);
    }

    private async Task<T?> PerformFetch<T>(TimeSpan? timeout = null)
    {
        var url = GetDownloadUrl();
        string resultContent;

        try
        {
            using var http = App.Storage.CreateHttpClientAsync(timeout);
            var result = await http.GetAsync(url).ConfigureAwait(false);
            resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JsonSerializer.Deserialize<T>(resultContent, RestfulFirebaseApp.DefaultJsonSerializerOption);

            result.EnsureSuccessStatusCode();

            return data;
        }
        catch (Exception ex)
        {
            throw new StorageUndefinedException(ex);
        }
    }

    private string GetTargetUrl()
    {
        return $"{FirebaseStorageEndpoint}{App.Config.CachedStorageBucket}/o?name={GetEscapedPath()}";
    }

    private string GetDownloadUrl()
    {
        return $"{FirebaseStorageEndpoint}{App.Config.CachedStorageBucket}/o/{GetEscapedPath()}";
    }

    private string GetFullDownloadUrl()
    {
        return GetDownloadUrl() + "?alt=media&token=";
    }

    private string GetEscapedPath()
    {
        return Uri.EscapeDataString(string.Join("/", children));
    }

    #endregion
}
