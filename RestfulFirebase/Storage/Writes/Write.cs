using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage.Writes;

/// <summary>
/// Provides progress tracker of the upload.
/// </summary>
public partial class Write
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used by this progress tracker.
    /// </summary>
    public FirebaseApp App { get; }

    /// <summary>
    /// Gets the <see cref="Common.Models.Progress"/> of the upload task.
    /// </summary>
    public Progress<Common.Models.Progress> Progress { get; private set; }

    /// <summary>
    /// Gets the target url of the upload file.
    /// </summary>
    public string TargetUrl { get; private set; }

    private const int ProgressReportDelayMiliseconds = 500;

    private readonly Task<HttpResponse<string>> uploadTask;
    private readonly Stream stream;

    internal Write(FirebaseApp app, string url, string downloadUrl, Stream stream, string? mimeType, IAuthorization? authorization, CancellationToken cancellationToken)
    {
        App = app;
        TargetUrl = url;
        uploadTask = UploadFile(url, downloadUrl, stream, mimeType, authorization, cancellationToken);
        this.stream = stream;
        Progress = new Progress<Common.Models.Progress>();

        Task.Factory.StartNew(ReportProgressLoop);
    }
}
