using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Storage.Models;

/// <summary>
/// Full list of meta data available here: https://firebase.google.com/docs/storage/web/file-metadata
/// </summary>
public class FileMetaData
{
    /// <summary>
    /// Gets or sets the bucket of the file metadata.
    /// </summary>
    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    /// <summary>
    /// Gets or sets the generation of the file metadata.
    /// </summary>
    [JsonPropertyName("generation")]
    public string? Generation { get; set; }

    /// <summary>
    /// Gets or sets the meta generation of the file metadata.
    /// </summary>
    [JsonPropertyName("metageneration")]
    public string? MetaGeneration { get; set; }

    /// <summary>
    /// Gets or sets the fullPath of the file metadata.
    /// </summary>
    [JsonPropertyName("fullPath")]
    public string? FullPath { get; set; }

    /// <summary>
    /// Gets or sets the name of the file metadata.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the size of the file metadata.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the time created of the file metadata.
    /// </summary>
    [JsonPropertyName("timeCreated")]
    public DateTime TimeCreated { get; set; }

    /// <summary>
    /// Gets or sets the time updated of the file metadata.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }

    /// <summary>
    /// Gets or sets the md5Hash of the file metadata.
    /// </summary>
    [JsonPropertyName("md5Hash")]
    public string? Md5Hash { get; set; }

    /// <summary>
    /// Gets or sets the cache control of the file metadata.
    /// </summary>
    [JsonPropertyName("cacheControl")]
    public string? CacheControl { get; set; }
    
    /// <summary>
    /// Gets or sets the content disposition of the file metadata.
    /// </summary>
    [JsonPropertyName("contentDisposition")]
    public string? ContentDisposition { get; set; }

    /// <summary>
    /// Gets or sets the content encoding of the file metadata.
    /// </summary>
    [JsonPropertyName("contentEncoding")]
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the content language of the file metadata.
    /// </summary>
    [JsonPropertyName("contentLanguage")]
    public string? ContentLanguage { get; set; }

    /// <summary>
    /// Gets or sets the content type of the file metadata.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata of the file metadata.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public Dictionary<string, object>? CustomMetadata { get; set; }
}
