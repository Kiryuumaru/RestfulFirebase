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

        public async Task Put(string jsonData, TimeSpan? timeout = null, Action<Exception> onException = null)
        {
            try
            {
                var c = GetClient(timeout);

                await Silent().SendAsync(c, jsonData, HttpMethod.Put);
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }
        }

        public async void Set(FirebaseProperty property, TimeSpan? timeout = null)
        {
            try
            {
                var query = new ChildQuery(this, () => property.Key, App);

                var blob = JsonConvert.SerializeObject(property.Data);
                var c = query.GetClient(timeout);

                property.SetStreamer(query);

                await query.Silent().SendAsync(c, blob, HttpMethod.Put);
            }
            catch (FirebaseException ex)
            {
                property.OnError(ex);
            }
        }

        public async void Set(FirebaseObject obj, TimeSpan? timeout = null)
        {
            try
            {
                var query = new ChildQuery(this, () => obj.Key, App);

                var collection = obj.GetRawPersistableProperties().ToDictionary(i => i.Key, i => i.Data);
                var data = JsonConvert.SerializeObject(collection);
                var c = query.GetClient(timeout);

                obj.SetStreamer(query);

                await query.Silent().SendAsync(c, data, HttpMethod.Put);
            }
            catch (FirebaseException ex)
            {
                obj.OnError(ex);
            }
        }

        public FirebaseProperty GetAsProperty(string path, TimeSpan? timeout = null)
        {
            var prop = FirebaseProperty.CreateFromKey(path);

            var query = new ChildQuery(this, () => path, App);
            prop.SetStreamer(query);

            return prop;
        }

        public FirebaseObject GetAsObject(string path, TimeSpan? timeout = null)
        {
            var obj = FirebaseObject.CreateFromKey(path);

            var query = new ChildQuery(this, () => path, App);
            obj.SetStreamer(query);

            return obj;
        }

        public FirebasePropertyGroup GetAsPropertyCollection(string path, TimeSpan? timeout = null)
        {
            var group = FirebasePropertyGroup.CreateFromKey(path);

            var query = new ChildQuery(this, () => path, App);
            group.SetStreamer(query);

            return group;
        }

        public FirebaseObjectGroup GetAsObjectCollection(string path, TimeSpan? timeout = null)
        {
            var group = FirebaseObjectGroup.CreateFromKey(path);

            var query = new ChildQuery(this, () => path, App);
            group.SetStreamer(query);

            return group;
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
