namespace RestfulFirebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using RestfulFirebase.Database.Streaming;

    using Newtonsoft.Json;
    using System.Net;
    using RestfulFirebase.Extensions.Http;
    using RestfulFirebase.Common;
    using RestfulFirebase.Common.Models;
    using System.Linq;

    /// <summary>
    /// Represents a firebase query. 
    /// </summary>
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        protected readonly TimeSpan DefaultHttpClientTimeout = new TimeSpan(0, 0, 180);

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
        /// Adds an auth parameter to the query.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="tokenFactory"> The auth token. </param>
        /// <returns> The <see cref="AuthQuery"/>. </returns>
        internal AuthQuery WithAuth(Func<string> tokenFactory)
        {
            return new AuthQuery(this, tokenFactory, App);
        }

        /// <summary>
        /// Appends print=silent to save bandwidth.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <returns> The <see cref="SilentQuery"/>. </returns>
        internal SilentQuery Silent()
        {
            return new SilentQuery(this, App);
        }

        #region Olds

        public Task PutAsync<T>(T obj)
        {
            return PutAsync(JsonConvert.SerializeObject(obj, App.Config.JsonSerializerSettings));
        }

        public Task PatchAsync<T>(T obj)
        {
            return PatchAsync(JsonConvert.SerializeObject(obj, App.Config.JsonSerializerSettings));
        }

        public async Task<FirebaseObject<T>> PostAsync<T>(T obj, bool generateKeyOffline = true)
        {
            var result = await PostAsync(JsonConvert.SerializeObject(obj, App.Config.JsonSerializerSettings), generateKeyOffline).ConfigureAwait(false);

            return new FirebaseObject<T>(result.Key, obj);
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

        #endregion

        #region News

        public Task SetAsync(ObservableProperty property, TimeSpan? timeout = null)
        {
            var collection = new Dictionary<string, string>() { { property.Key, property.Data } };
            var data = JsonConvert.SerializeObject(collection, App.Config.JsonSerializerSettings);
            var c = GetClient(timeout);
            return Silent().SendAsync(c, data, new HttpMethod("PATCH"));
        }
        
        public Task SetAsync(Storable storable, TimeSpan? timeout = null)
        {
            var query = new ChildQuery(this, () => storable.Id, App);

            var collection = storable.GetRawProperties().ToDictionary(i => i.Key, i => i.Data);
            var data = JsonConvert.SerializeObject(collection, query.App.Config.JsonSerializerSettings);
            var c = query.GetClient(timeout);
            return query.Silent().SendAsync(c, data, new HttpMethod("PATCH"));
        }

        public async Task<ObservableProperty> GetAsPropertyAsync(string path, TimeSpan? timeout = null)
        {
            var query = string.IsNullOrEmpty(path) ? this : new ChildQuery(this, () => path, App);
            var c = query.GetClient(timeout);

            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await query.BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex);
            }

            try
            {
                var response = await c.GetAsync(url).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                response.Dispose();

                var data = JsonConvert.DeserializeObject<string>(responseData, query.App.Config.JsonSerializerSettings);
                return new ObservableProperty(data, path);
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, string.Empty, responseData, statusCode, ex);
            }
        }

        public async Task<Storable> GetAsStorableAsync(string path, TimeSpan? timeout = null)
        {
            var query = string.IsNullOrEmpty(path) ? this : new ChildQuery(this, () => path, App);
            var c = query.GetClient(timeout);

            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await query.BuildUrlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex);
            }

            try
            {
                var response = await c.GetAsync(url).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                response.Dispose();

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData, query.App.Config.JsonSerializerSettings);
                return new Storable(path, data.Select(i => new ObservableProperty(i.Key, i.Value)));
            }
            catch (Exception ex)
            {
                throw new FirebaseException(url, string.Empty, responseData, statusCode, ex);
            }
        }

        public async Task DeleteAsync(string path, TimeSpan? timeout = null)
        {
            var query = string.IsNullOrEmpty(path) ? this : new ChildQuery(this, () => path, App);
            var c = query.GetClient(timeout);

            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await query.BuildUrlAsync().ConfigureAwait(false);
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

        #endregion

        /// <summary>
        /// Builds the actual URL of this query.
        /// </summary>
        /// <returns> The <see cref="string"/>. </returns>
        public async Task<string> BuildUrlAsync()
        {
            if (App.Auth.Authenticated)
            {
                return await Task.Run(delegate
                {
                    return WithAuth(() => App.Auth.GetFreshTokenAsync().Result).BuildUrl(null);
                });
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
                var key = Helpers.GenerateSafeUID();
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

            return Silent().SendAsync(c, data, new HttpMethod("PATCH"));
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

            return Silent().SendAsync(c, data, HttpMethod.Put);
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
                client = App.Config.HttpClientFactory.GetHttpClient(timeout ?? DefaultHttpClientTimeout);
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
