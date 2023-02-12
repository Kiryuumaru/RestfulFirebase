using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using RestfulFirebase.Storage.Writes;
using RestfulFirebase.Storage.Models;
using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using TransactionHelpers;
using TransactionHelpers.Interface;

namespace RestfulFirebase.Storage.References;

public partial class Reference
{
    /// <summary>
    /// Creates new instance of <see cref="Reference"/> child reference.
    /// </summary>
    /// <param name="childRoot">
    /// The child reference name or file name.
    /// </param>
    /// <returns>
    /// The instance of <see cref="Reference"/> child reference.
    /// </returns>
    public Reference Child(string childRoot)
    {
        return new Reference(Bucket, this, childRoot);
    }

    /// <summary>
    /// Starts uploading given stream to target location.
    /// </summary>
    /// <param name="stream">
    /// Stream to upload.
    /// </param>
    /// <param name="mimeType">
    /// Optional type of data being uploaded, will be used to set HTTP Content-Type header.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token which can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Write"/> which can be used to track the progress of the upload.
    /// </returns>
    public Write Write(Stream stream, string? mimeType = null, IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        return new Write(App, GetTargetUrl(), GetFullDownloadUrl(), stream, mimeType, authorization, cancellationToken);
    }

    /// <summary>
    /// Gets the meta data for given file.
    /// </summary>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> of the request.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> which results a <see cref="FileMetaData"/> of this reference.
    /// </returns>
    public Task<HttpResponse<FileMetaData>> GetMetaData(IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        return App.Storage.ExecuteGet<FileMetaData>(GetDownloadUrl(), authorization, cancellationToken);
    }

    /// <summary>
    /// Gets the meta data for given file.
    /// </summary>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> of the request.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> which results a <see cref="string"/> that represents the download url of the reference.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<string>> GetDownloadUrl(IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        HttpResponse<string> response = new();

        var getResponse = await App.Storage.ExecuteGet<Dictionary<string, object>>(GetDownloadUrl(), authorization, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            return response;
        }

        if (getResponse.Result == null || !getResponse.Result.TryGetValue("downloadTokens", out object? downloadTokens))
        {
            response.Append(new ArgumentOutOfRangeException($"Could not extract 'downloadTokens' property from response. Response: {JsonSerializer.Serialize(getResponse.Result)}"));
            return response;
        }

        response.Append(GetFullDownloadUrl() + downloadTokens);

        return response;
    }

    /// <summary>
    /// Deletes the file of this reference.
    /// </summary>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> of the request.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents a proxy for the task returned by deletion.
    /// </returns>
    public async Task<HttpResponse> Delete(IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var getResponse = await App.Storage.ExecuteDelete(authorization, GetDownloadUrl(), cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            return response;
        }

        return response;
    }
}
