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

        public async Task Put(string jsonData, TimeSpan? timeout = null, Action<FirebaseException> onException = null)
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
                onException?.Invoke(new FirebaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex));
                return;
            }

            if (jsonData == null)
            {
                var c = GetClient(timeout);

                try
                {
                    var result = await c.DeleteAsync(url).ConfigureAwait(false);
                    statusCode = result.StatusCode;
                    responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                }
            }
            else
            {
                try
                {
                    var c = GetClient(timeout);

                    await Silent().SendAsync(c, jsonData, HttpMethod.Put);
                }
                catch (Exception ex)
                {
                    onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                }
            }
        }

        public RealtimeHolder<FirebaseProperty> SetStream(FirebaseProperty property)
        {
            var query = new ChildQuery(this, () => property.Key, App);
            return new RealtimeHolder<FirebaseProperty>(property, delegate { property.SetRealtime(query, true); });
        }

        public RealtimeHolder<FirebaseObject> SetStream(FirebaseObject obj)
        {
            var query = new ChildQuery(this, () => obj.Key, App);
            return new RealtimeHolder<FirebaseObject>(obj, delegate { obj.SetRealtime(query, true); });
        }

        public RealtimeHolder<FirebaseProperty> GetStreamAsProperty(string path)
        {
            var prop = FirebaseProperty.CreateFromKey(path);
            var query = new ChildQuery(this, () => path, App);
            return new RealtimeHolder<FirebaseProperty>(prop, delegate { prop.SetRealtime(query, false); });
        }

        public RealtimeHolder<FirebaseObject> GetStreamAsObject(string path)
        {
            var obj = FirebaseObject.CreateFromKey(path);
            var query = new ChildQuery(this, () => path, App);
            return new RealtimeHolder<FirebaseObject>(obj, delegate { obj.SetRealtime(query, false); });
        }

        public RealtimeHolder<FirebasePropertyGroup> GetStreamAsPropertyCollection(string path)
        {
            var group = FirebasePropertyGroup.CreateFromKey(path);
            var query = new ChildQuery(this, () => path, App);
            return new RealtimeHolder<FirebasePropertyGroup>(group, delegate { group.SetRealtime(query, false); });
        }

        public RealtimeHolder<FirebaseObjectGroup> GetStreamAsObjectCollection(string path)
        {
            var group = FirebaseObjectGroup.CreateFromKey(path);
            var query = new ChildQuery(this, () => path, App);
            return new RealtimeHolder<FirebaseObjectGroup>(group, delegate { group.SetRealtime(query, false); });
        }

        public void Delete(string path)
        {

        }

        public async Task<string> BuildUrlAsync()
        {
            if (App.Auth.Authenticated)
            {
                return await Task.Run(delegate
                {
                    return WithAuth(() =>
                    {
                        var getTokenResult = App.Auth.GetFreshTokenAsync();
                        if (!getTokenResult.Result.IsSuccess) throw getTokenResult.Result.Exception;
                        return getTokenResult.Result.Result;
                    }).BuildUrl(null);
                });
            }

            return BuildUrl(null);
        }

        public string GetAbsolutePath()
        {
            var url = BuildUrlSegment(this);

            if (Parent != null)
            {
                url = Parent.BuildUrl(this) + url;
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
