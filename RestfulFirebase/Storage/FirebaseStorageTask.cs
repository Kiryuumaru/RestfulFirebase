using Newtonsoft.Json;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage
{
    /// <summary>
    /// Provides progress tracker of the upload.
    /// </summary>
    public class FirebaseStorageTask
    {
        #region Properties

        private const int ProgressReportDelayMiliseconds = 500;

        private readonly Task<string> uploadTask;
        private readonly Stream stream;

        /// <summary>
        /// Gets the <see cref="RestfulFirebaseApp"/> used by this progress tracker.
        /// </summary>
        public RestfulFirebaseApp App { get; }

        /// <summary>
        /// Gets the <see cref="FirebaseStorageProgress"/> of the upload task.
        /// </summary>
        public Progress<FirebaseStorageProgress> Progress { get; private set; }

        /// <summary>
        /// Gets the target url of the upload file.
        /// </summary>
        public string TargetUrl { get; private set; }

        #endregion

        #region Initializers

        internal FirebaseStorageTask(RestfulFirebaseApp app, string url, string downloadUrl, Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            App = app;
            TargetUrl = url;
            uploadTask = UploadFile(url, downloadUrl, stream, cancellationToken, mimeType);
            this.stream = stream;
            Progress = new Progress<FirebaseStorageProgress>();

            Task.Factory.StartNew(ReportProgressLoop);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the awaiter of the specified upload task.
        /// </summary>
        /// <returns>
        /// The awaiter of the specified upload task.
        /// </returns>
        public TaskAwaiter<string> GetAwaiter()
        {
            return uploadTask.GetAwaiter();
        }

        private async Task<string> UploadFile(string url, string downloadUrl, Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            var responseData = "N/A";

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
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                return downloadUrl + data["downloadTokens"];
            }
        }

        private async void ReportProgressLoop()
        {
            while (!uploadTask.IsCompleted)
            {
                await Task.Delay(ProgressReportDelayMiliseconds).ConfigureAwait(false);

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

        #endregion
    }
}
