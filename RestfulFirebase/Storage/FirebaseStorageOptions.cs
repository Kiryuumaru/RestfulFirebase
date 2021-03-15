namespace RestfulFirebase.Storage
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class FirebaseStorageOptions
    {
        /// <summary>
        /// Gets or sets the method for retrieving auth tokens. Default is null.
        /// </summary>
        public Func<Task<string>> AuthTokenAsyncFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether <see cref="TaskCanceledException"/> should be thrown when cancelling a running <see cref="FirebaseStorageTask"/>.
        /// </summary>
        public bool ThrowOnCancel
        {
            get;
            set;
        }

        /// <summary>
        /// Timeout of the <see cref="HttpClient"/>. Default is 100s.
        /// </summary>
        public TimeSpan HttpClientTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> with authentication header when <see cref="FirebaseStorageOptions.AuthTokenAsyncFactory"/> is specified.
        /// </summary>
        /// <param name="options">Firebase storage options.</param>
        public async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = new HttpClient();

            if (HttpClientTimeout != default)
            {
                client.Timeout = HttpClientTimeout;
            }

            if (AuthTokenAsyncFactory != null)
            {
                var auth = await AuthTokenAsyncFactory().ConfigureAwait(false);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Firebase", auth);
            }

            return client;
        }
    }
}
