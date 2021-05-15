using RestfulFirebase.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage
{
    public class FirebaseStorageReference
    {
        private const string FirebaseStorageEndpoint = "https://firebasestorage.googleapis.com/v0/b/";

        private readonly List<string> children;

        public RestfulFirebaseApp App { get; }

        internal FirebaseStorageReference(RestfulFirebaseApp app, string childRoot)
        {
            children = new List<string>();

            App = app;
            children.Add(childRoot);
        }

        public FirebaseStorageTask PutAsync(Stream stream, CancellationToken cancellationToken, string mimeType = null)
        {
            return new FirebaseStorageTask(App, GetTargetUrl(), GetFullDownloadUrl(), stream, cancellationToken, mimeType);
        }

        public FirebaseStorageTask PutAsync(Stream fileStream)
        {
            return PutAsync(fileStream, CancellationToken.None);
        }

        public async Task<FirebaseMetaData> GetMetaDataAsync(TimeSpan? timeout = null)
        {
            var data = await PerformFetch<FirebaseMetaData>(timeout);

            return data;
        }

        public async Task<string> GetDownloadUrlAsync(TimeSpan? timeout = null)
        {
            var data = await PerformFetch<Dictionary<string, object>>(timeout);

            if (!data.TryGetValue("downloadTokens", out object downloadTokens))
            {
                throw new ArgumentOutOfRangeException($"Could not extract 'downloadTokens' property from response. Response: {JsonSerializer.Serialize(data, Utils.JsonSerializerOptions)}");
            }

            return GetFullDownloadUrl() + downloadTokens;
        }

        public async Task DeleteAsync(TimeSpan? timeout = null)
        {
            var url = GetDownloadUrl();
            var resultContent = "N/A";

            try
            {
                using (var http = App.Storage.CreateHttpClientAsync(timeout))
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

        public FirebaseStorageReference Child(string name)
        {
            children.Add(name);
            return this;
        }

        private async Task<T> PerformFetch<T>(TimeSpan? timeout = null)
        {
            var url = GetDownloadUrl();
            var resultContent = "N/A";

            try
            {
                using (var http = App.Storage.CreateHttpClientAsync(timeout))
                {
                    var result = await http.GetAsync(url);
                    resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var data = JsonSerializer.Deserialize<T>(resultContent, Utils.JsonSerializerOptions);

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
