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

        #region News

        public async Task SetAsync(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            try
            {
                var query = new ChildQuery(this, () => property.Key, App);

                var data = JsonConvert.SerializeObject(property.Data, App.Config.JsonSerializerSettings);
                var c = query.GetClient(timeout);

                await query.Silent().SendAsync(c, data, HttpMethod.Put);
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        public async void Set(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            await SetAsync(property, timeout, onException);
        }
        
        public async Task SetAsync(FirebaseObject storable, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            try
            {
                var query = new ChildQuery(this, () => storable.Key, App);

                var collection = storable.GetPersistableRawProperties().ToDictionary(i => i.Key, i => i.Data);
                var data = JsonConvert.SerializeObject(collection, query.App.Config.JsonSerializerSettings);
                var c = query.GetClient(timeout);



                await query.Silent().SendAsync(c, data, HttpMethod.Put);
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        public async void Set(FirebaseObject storable, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            await SetAsync(storable, timeout, onException);
        }

        public async Task<FirebaseProperty<T>> GetAsPropertyAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            var query = new ChildQuery(this, () => path, App);
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
                onException?.Invoke(new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex));
                return null;
            }

            try
            {
                var response = await c.GetAsync(url).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                response.Dispose();

                var data = JsonConvert.DeserializeObject<string>(responseData, query.App.Config.JsonSerializerSettings);
                var prop = FirebaseProperty.CreateFromKeyAndData<T>(path, data);



                return prop;
            }
            catch (Exception ex)
            {
                onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                return null;
            }
        }

        public async Task<T> GetAsStorableAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null)
            where T : FirebaseObject
        {
            var query = new ChildQuery(this, () => path, App);
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
                onException?.Invoke(new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex));
                return null;
            }

            try
            {
                var response = await c.GetAsync(url).ConfigureAwait(false);
                statusCode = response.StatusCode;
                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                response.Dispose();

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData, query.App.Config.JsonSerializerSettings);
                var obj = FirebaseObject.CreateFromKeyAndProperties(path, data.Select(i => DistinctProperty.CreateFromKeyAndData(i.Key, i.Value)));


                
                return obj.Parse<T>();
            }
            catch (Exception ex)
            {
                onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                return null;
            }
        }

        public async void DeleteAsync(string path, TimeSpan? timeout = null)
        {
            var query = new ChildQuery(this, () => path, App);
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
