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
using RestfulFirebase.Database.Models;
using System.IO;

namespace RestfulFirebase.Database.Query
{
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        protected readonly TimeSpan DefaultHttpClientTimeout = new TimeSpan(0, 0, 180);

        protected readonly FirebaseQuery Parent;

        private IHttpClientProxy client;

        protected FirebaseQuery(FirebaseQuery parent, RestfulFirebaseApp app)
        {
            App = app;
            Parent = parent;
        }

        public RestfulFirebaseApp App { get; }

        internal AuthQuery WithAuth(Func<string> tokenFactory)
        {
            return new AuthQuery(this, tokenFactory, App);
        }

        internal SilentQuery Silent()
        {
            return new SilentQuery(this, App);
        }

        public async Task SetAsync(FirebaseProperty property, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            try
            {
                var query = new ChildQuery(this, () => property.Key, App);

                var data = JsonConvert.SerializeObject(property.Data, App.Config.JsonSerializerSettings);
                var c = query.GetClient(timeout);

                property.RealtimeSubscription = Observable
                    .Create<StreamEvent>(observer => new NodeStreamer(observer, query).Run())
                    .Subscribe(stream => property.ConsumePersistableStream(stream));

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
        
        public async Task SetAsync(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            try
            {
                var query = new ChildQuery(this, () => obj.Key, App);

                var collection = obj.GetRawPersistableProperties().ToDictionary(i => i.Key, i => i.Data);
                var data = JsonConvert.SerializeObject(collection, query.App.Config.JsonSerializerSettings);
                var c = query.GetClient(timeout);

                obj.RealtimeWirePath = query.GetAbsolutePath();
                obj.RealtimeSubscription = Observable
                    .Create<StreamEvent>(observer => new NodeStreamer(observer, query).Run())
                    .Subscribe(stream => obj.ConsumePersistableStream(stream));

                await query.Silent().SendAsync(c, data, HttpMethod.Put);
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        public async void Set(FirebaseObject obj, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            await SetAsync(obj, timeout, onException);
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

                prop.RealtimeWirePath = query.GetAbsolutePath();
                prop.RealtimeSubscription = Observable
                    .Create<StreamEvent>(observer => new NodeStreamer(observer, query).Run())
                    .Subscribe(stream => prop.ConsumePersistableStream(stream));

                return prop;
            }
            catch (Exception ex)
            {
                onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                return null;
            }
        }

        public async Task<T> GetAsObjectAsync<T>(string path, TimeSpan? timeout = null, Action<Exception> onException = null)
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

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
                var props = data.Select(i => DistinctProperty.CreateFromKeyAndData(i.Key, i.Value));
                var obj = FirebaseObject.CreateFromKeyAndProperties(path, props);

                obj.RealtimeWirePath = query.GetAbsolutePath();
                obj.RealtimeSubscription = Observable
                    .Create<StreamEvent>(observer => new NodeStreamer(observer, query).Run())
                    .Subscribe(stream => obj.ConsumePersistableStream(stream));

                return obj.Parse<T>();
            }
            catch (Exception ex)
            {
                onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                return null;
            }
        }

        public async Task<FirebasePropertyGroup> GetAsPropertyCollectionAsync(string path, TimeSpan? timeout = null, Action<Exception> onException = null)
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

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
                var props = data.Select(i => DistinctProperty.CreateFromKeyAndData(i.Key, i.Value));
                var obj = FirebaseObject.CreateFromKeyAndProperties(path, props);

                obj.RealtimeWirePath = query.GetAbsolutePath();
                obj.RealtimeSubscription = Observable
                    .Create<StreamEvent>(observer => new NodeStreamer(observer, query).Run())
                    .Subscribe(stream => obj.ConsumePersistableStream(stream));

                //return obj.Parse<T>();
                return null;
            }
            catch (Exception ex)
            {
                onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                return null;
            }
        }

        public async Task<FirebaseObjectGroup> GetAsObjectCollectionAsync(string path, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            return default;
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

        public string GetAbsolutePath()
        {
            var url = BuildUrlSegment(this);

            if (Parent != null)
            {
                url = Path.Combine(Parent.BuildUrl(this), url);
            }

            return url;
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        protected abstract string BuildUrlSegment(FirebaseQuery child);

        private string BuildUrl(FirebaseQuery child)
        {
            var url = BuildUrlSegment(child);

            if (Parent != null)
            {
                url = Path.Combine(Parent.BuildUrl(this), url);
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
