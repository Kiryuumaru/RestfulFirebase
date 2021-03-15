namespace RestfulFirebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using RestfulFirebase.Database.Offline;
    using RestfulFirebase.Database.Streaming;

    using Newtonsoft.Json;
    using System.Net;
    using RestfulFirebase.Extensions.Http;

    /// <summary>
    /// Represents a firebase query. 
    /// </summary>
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        protected TimeSpan DEFAULT_HTTP_CLIENT_TIMEOUT = new TimeSpan(0, 0, 180);

        protected readonly FirebaseQuery Parent;

        private IHttpClientProxy client;

        /// <summary> 
        /// Initializes a new instance of the <see cref="FirebaseQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent of this query. </param>
        /// <param name="app"> The owner. </param>
        protected FirebaseQuery(FirebaseQuery parent, RestfulFirebaseApp app)
        {
            App = app;
            Parent = parent;
        }

        /// <summary>
        /// Gets the app.
        /// </summary>
        public RestfulFirebaseApp App
        {
            get;
        }

        /// <summary>
        /// Queries the firebase server once returning collection of items.
        /// </summary>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Collection of <see cref="FirebaseObject{T}"/> holding the entities returned by server. </returns>
        public async Task<IReadOnlyCollection<FirebaseObject<T>>> OnceAsync<T>(TimeSpan? timeout = null)
        {
            string url;

            try
            {
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", string.Empty, string.Empty, HttpStatusCode.OK, ex);
            }

            return await GetClient(timeout).GetObjectCollectionAsync<T>(url, App.Config.JsonSerializerSettings)
                .ConfigureAwait(false);
        }


        /// <summary>
        /// Assumes given query is pointing to a single object of type <typeparamref name="T"/> and retrieves it.
        /// </summary>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <returns> Single object of type <typeparamref name="T"/>. </returns>
        public async Task<T> OnceSingleAsync<T>(TimeSpan? timeout = null)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex);
            }

            try
            {
                var response = await GetClient(timeout).GetAsync(url).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                response.Dispose();

                return JsonConvert.DeserializeObject<T>(responseData, App.Config.JsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, string.Empty, responseData, statusCode, ex);
            }
        }

        /// <summary>
        /// Starts observing this query watching for changes real time sent by the server.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <param name="exceptionHandler"> Optional exception handler for the stream subscription. </param>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <returns> Observable stream of <see cref="FirebaseEvent{T}"/>. </returns>
        public IObservable<FirebaseEvent<T>> AsObservable<T>(EventHandler<ContinueExceptionEventArgs<FirebaseException>> exceptionHandler = null, string elementRoot = "")
        {
            return Observable.Create<FirebaseEvent<T>>(observer =>
            {
                var sub = new FirebaseSubscription<T>(observer, this, elementRoot, new FirebaseCache<T>());
                sub.ExceptionThrown += exceptionHandler;
                return sub.Run();
            });
        }

        /// <summary>
        /// Builds the actual URL of this query.
        /// </summary>
        /// <returns> The <see cref="string"/>. </returns>
        public async Task<string> BuildUrlAsync()
        {
            if (App.Auth.Authenticated)
            {
                await App.Auth.GetFreshAuthAsync();
                var token = App.Auth.FirebaseToken;
                return this.WithAuth(token).BuildUrl(null);
            }

            return BuildUrl(null);
        }

        /// <summary>
        /// Posts given object to repository.
        /// </summary>
        /// <param name="data"> The json data. </param>
        /// <param name="generateKeyOffline"> Specifies whether the key should be generated offline instead of online. </param>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <returns> Resulting firebase object with populated key. </returns>
        public async Task<FirebaseObject<string>> PostAsync(string data, bool generateKeyOffline = true, TimeSpan? timeout = null)
        {
            // post generates a new key server-side, while put can be used with an already generated local key
            if (generateKeyOffline)
            {
                var key = FirebaseKeyGenerator.Next();
                await new ChildQuery(this, () => key, App).PutAsync(data).ConfigureAwait(false);

                return new FirebaseObject<string>(key, data);
            }
            else
            {
                var c = GetClient(timeout);
                var sendData = await SendAsync(c, data, HttpMethod.Post).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<PostResult>(sendData, App.Config.JsonSerializerSettings);

                return new FirebaseObject<string>(result.Name, data);
            }
        }

        /// <summary>
        /// Patches data at given location instead of overwriting them.
        /// </summary> 
        /// <param name="data"> The json data. </param>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <returns> The <see cref="Task"/>. </returns>
        public Task PatchAsync(string data, TimeSpan? timeout = null)
        {
            var c = GetClient(timeout);

            return this.Silent().SendAsync(c, data, new HttpMethod("PATCH"));
        }

        /// <summary>
        /// Sets or overwrites data at given location.
        /// </summary> 
        /// <param name="data"> The json data. </param>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <returns> The <see cref="Task"/>. </returns>
        public Task PutAsync(string data, TimeSpan? timeout = null)
        {
            var c = GetClient(timeout);

            return this.Silent().SendAsync(c, data, HttpMethod.Put);
        }

        /// <summary>
        /// Deletes data from given location.
        /// </summary>
        /// <param name="timeout"> Optional timeout value. </param>
        /// <returns> The <see cref="Task"/>. </returns>
        public async Task DeleteAsync(TimeSpan? timeout = null)
        {
            string url;
            var c = GetClient(timeout);
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex);
            }

            try
            {
                var result = await c.DeleteAsync(url).ConfigureAwait(false);
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, string.Empty, responseData, statusCode, ex);
            }
        }

        /// <summary>
        /// Disposes this instance.  
        /// </summary>
        public void Dispose()
        {
            client?.Dispose();
        }

        /// <summary>
        /// Build the url segment of this child.
        /// </summary>
        /// <param name="child"> The child of this query. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected abstract string BuildUrlSegment(FirebaseQuery child);

        private string BuildUrl(FirebaseQuery child)
        {
            var url = BuildUrlSegment(child);

            if (Parent != null)
            {
                url = Parent.BuildUrl(this) + url;
            }

            return url;
        }

        private HttpClient GetClient(TimeSpan? timeout = null)
        {
            if (client == null)
            {
                client = App.Config.HttpClientFactory.GetHttpClient(timeout ?? DEFAULT_HTTP_CLIENT_TIMEOUT);
            }

            return client.GetHttpClient();
        }

        private async Task<string> SendAsync(HttpClient client, string data, HttpMethod method)
        {
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;
            var requestData = data;
            string url;

            try
            {
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", requestData, responseData, statusCode, ex);
            }

            var message = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(requestData)
            };

            try
            {
                var result = await client.SendAsync(message).ConfigureAwait(false);
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return responseData;
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, requestData, responseData, statusCode, ex);
            }
        }
    }
}
