namespace RestfulFirebase.Storage
{
    using Newtonsoft.Json;
    using RestfulFirebase.Extensions.Http;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class FirebaseStorageReference
    {
        private const string FirebaseStorageEndpoint = "https://firebasestorage.googleapis.com/v0/b/";

        private readonly RestfulFirebaseApp app;
        private readonly List<string> children;

        /// <summary>
        /// Gets the RestfulFirebaseApp
        /// </summary>
        public RestfulFirebaseApp App { get; }

        internal FirebaseStorageReference(RestfulFirebaseApp app, string childRoot)
        {
            children = new List<string>();

            App = app;
            children.Add(childRoot);
        }

        /// <summary>
        /// Starts uploading given stream to target location.
        /// </summary>
        /// <param name="stream"> Stream to upload. </param>
        /// <param name="cancellationToken"> Cancellation token which can be used to cancel the operation. </param>
        /// <param name="mimeType"> Optional type of data being uploaded, will be used to set HTTP Content-Type header. </param>
        /// <returns> <see cref="FirebaseStorageTask"/> which can be used to track the progress of the upload. </returns>
        public FirebaseStorageTask PutAsync(Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            return new FirebaseStorageTask(App, GetTargetUrl(), GetFullDownloadUrl(), stream, cancellationToken, mimeType);
        }

        /// <summary>
        /// Starts uploading given stream to target location.
        /// </summary>
        /// <param name="stream"> Stream to upload. </param>
        /// <returns> <see cref="FirebaseStorageTask"/> which can be used to track the progress of the upload. </returns>
        public FirebaseStorageTask PutAsync(Stream fileStream)
        {
            return PutAsync(fileStream, CancellationToken.None);
        }

        /// <summary>
        /// Gets the meta data for given file.
        /// </summary>
        /// <returns></returns>
        public async Task<FirebaseMetaData> GetMetaDataAsync()
        {
            var data = await PerformFetch<FirebaseMetaData>();

            return data;
        }

        /// <summary>
        /// Gets the url to download given file.
        /// </summary>
        public async Task<string> GetDownloadUrlAsync()
        {
            var data = await PerformFetch<Dictionary<string, object>>();

            object downloadTokens;

            if (!data.TryGetValue("downloadTokens", out downloadTokens))
            {
                throw new ArgumentOutOfRangeException($"Could not extract 'downloadTokens' property from response. Response: {JsonConvert.SerializeObject(data)}");
            }

            return GetFullDownloadUrl() + downloadTokens;
        }

        /// <summary>
        /// Deletes a file at target location.
        /// </summary>
        public async Task DeleteAsync()
        {
            var url = GetDownloadUrl();
            var resultContent = "N/A";

            try
            {
                using (var http = await App.Config.CreateHttpClientAsync().ConfigureAwait(false))
                {
                    var result = await http.DeleteAsync(url).ConfigureAwait(false);

                    resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                throw new FirebaseStorageException(url, resultContent, ex);
            }
        }

        /// <summary>
        /// Constructs firebase path to the file.
        /// </summary>
        /// <param name="name"> Name of the entity. This can be folder or a file name or full path.</param>
        /// <example>
        ///     storage
        ///         .Child("some")
        ///         .Child("path")
        ///         .Child("to/file.png");
        /// </example>
        /// <returns> <see cref="FirebaseStorageReference"/> for fluid syntax. </returns>
        public FirebaseStorageReference Child(string name)
        {
            children.Add(name);
            return this;
        }

        private async Task<T> PerformFetch<T>()
        {
            var url = GetDownloadUrl();
            var resultContent = "N/A";

            try
            {
                using (var http = await App.Config.CreateHttpClientAsync().ConfigureAwait(false))
                {
                    var result = await http.GetAsync(url);
                    resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<T>(resultContent);

                    result.EnsureSuccessStatusCode();

                    return data;
                }
            }
            catch (Exception ex)
            {
                throw new FirebaseStorageException(url, resultContent, ex);
            }
        }

        private string GetTargetUrl()
        {
            return $"{FirebaseStorageEndpoint}{App.Config.StorageBucket}/o?name={GetEscapedPath()}";
        }

        private string GetDownloadUrl()
        {
            return $"{FirebaseStorageEndpoint}{App.Config.StorageBucket}/o/{GetEscapedPath()}";
        }

        private string GetFullDownloadUrl()
        {
            return GetDownloadUrl() + "?alt=media&token=";
        }

        private string GetEscapedPath()
        {
            return Uri.EscapeDataString(string.Join("/", children));
        }
    }
}
