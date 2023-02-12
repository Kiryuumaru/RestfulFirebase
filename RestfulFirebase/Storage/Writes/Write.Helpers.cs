using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Utilities;
using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage.Writes;

public partial class Write
{
    private async Task<HttpResponse<string>> UploadFile(string url, string downloadUrl, Stream stream, string? mimeType, IAuthorization? authorization, CancellationToken cancellationToken)
    {
        HttpResponse<string> response = new();

        var getHttpClientResponse = await App.Storage.GetHttpClient(authorization, cancellationToken);
        response.Append(getHttpClientResponse);
        if (getHttpClientResponse.IsError)
        {
            return response;
        }

        stream.Seek(0L, SeekOrigin.Begin);
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url)
        {
            Content = new StreamContent(stream)
        };

        if (!string.IsNullOrEmpty(mimeType))
        {
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        }

        var executePostResponse = await App.Storage.Execute<Dictionary<string, object>>(httpRequestMessage, authorization, cancellationToken);
        response.Append(executePostResponse);
        if (executePostResponse.IsError)
        {
            return response;
        }

        response.Append(downloadUrl + executePostResponse.Result["downloadTokens"]);

        return response;
    }

    private async void ReportProgressLoop()
    {
        while (!uploadTask.IsCompleted)
        {
            await Task.Delay(ProgressReportDelayMiliseconds).ConfigureAwait(false);

            try
            { 
                OnReportProgress(new Common.Models.Progress(stream.Position, stream.Length));
            }
            catch (ObjectDisposedException)
            {
                // there is no 100 % way to prevent ObjectDisposedException, there are bound to be concurrency issues.
                return;
            } 
        }
    }

    private void OnReportProgress(Common.Models.Progress progress)
    {
        (Progress as IProgress<Common.Models.Progress>).Report(progress);
    }
}
