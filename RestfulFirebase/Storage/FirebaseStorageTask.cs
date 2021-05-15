using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage
{
    public class FirebaseStorageTask
    {
        private const int ProgressReportDelayMiliseconds = 500;

        private readonly Task<string> uploadTask;
        private readonly Stream stream;

        public RestfulFirebaseApp App { get; }

        public FirebaseStorageTask(RestfulFirebaseApp app, string url, string downloadUrl, Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            App = app;
            TargetUrl = url;
            uploadTask = UploadFile(url, downloadUrl, stream, cancellationToken, mimeType);
            this.stream = stream;
            Progress = new Progress<FirebaseStorageProgress>();

            Task.Factory.StartNew(ReportProgressLoop);
        }

        public Progress<FirebaseStorageProgress> Progress
        {
            get;
            private set;
        }


        public string TargetUrl
        {
            get;
            private set;
        }

        public TaskAwaiter<string> GetAwaiter()
        {
            return uploadTask.GetAwaiter();
        }

        private async Task<string> UploadFile(string url, string downloadUrl, Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            var responseData = "N/A";

            try
            {
                using (var client = App.Storage.CreateHttpClientAsync())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StreamContent(stream)
                    };

                    if (!string.IsNullOrEmpty(mimeType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                    }

                    var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(responseData, Utils.JsonSerializerOptions);

                    return downloadUrl + data["downloadTokens"];
                }
            }
            catch (TaskCanceledException)
            {
                if (App.Config.StorageThrowOnCancel)
                {
                    throw;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new FirebaseStorageException(url, responseData, ex);
            }
        }

        private async void ReportProgressLoop()
        {
            while (!uploadTask.IsCompleted)
            {
                await Task.Delay(ProgressReportDelayMiliseconds);

                try
                { 
                    OnReportProgress(new FirebaseStorageProgress(stream.Position, stream.Length));
                }
                catch (ObjectDisposedException)
                {
                    // there is no 100 % way to prevent ObjectDisposedException, there are bound to be concurrency issues.
                    return;
                } 
            }
        }

        private void OnReportProgress(FirebaseStorageProgress progress)
        {
            (Progress as IProgress<FirebaseStorageProgress>).Report(progress);
        }
    }
}
