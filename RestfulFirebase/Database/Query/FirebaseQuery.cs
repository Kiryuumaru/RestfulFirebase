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

        protected FirebaseQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        {
            App = app;
            Parent = parent;
        }

        public RestfulFirebaseApp App { get; }

        internal AuthQuery WithAuth(Func<string> tokenFactory)
        {
            return new AuthQuery(App, this, tokenFactory);
        }

        internal SilentQuery Silent()
        {
            return new SilentQuery(App, this);
        }

        public async void Put(string jsonData, TimeSpan? timeout = null, Action<FirebaseException> onException = null)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (FirebaseException ex)
            {
                onException?.Invoke(ex);
                return;
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
                catch (FirebaseException ex)
                {
                    onException?.Invoke(ex);
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
                catch (FirebaseException ex)
                {
                    onException?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    onException?.Invoke(new FirebaseException(url, string.Empty, responseData, statusCode, ex));
                }
            }
        }

        public RealtimeHolder<FirebaseProperty<T>> AsRealtimeProperty<T>(FirebaseProperty<T> prop)
        {
            var query = new ChildQuery(App, this, () => prop.Key);
            return new RealtimeHolder<FirebaseProperty<T>>(prop, query);
        }

        public RealtimeHolder<FirebaseProperty<T>> AsRealtimeProperty<T>(string path)
        {
            var prop = FirebaseProperty.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<FirebaseProperty<T>>(prop.ParseModel<T>(), query);
        }

        public RealtimeHolder<FirebaseProperty> AsRealtimeProperty(FirebaseProperty prop)
        {
            var query = new ChildQuery(App, this, () => prop.Key);
            return new RealtimeHolder<FirebaseProperty>(prop, query);
        }

        public RealtimeHolder<FirebaseProperty> AsRealtimeProperty(string path)
        {
            var prop = FirebaseProperty.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<FirebaseProperty>(prop, query);
        }

        public RealtimeHolder<T> AsRealtimeObject<T>(T obj)
            where T : FirebaseObject
        {
            var query = new ChildQuery(App, this, () => obj.Key);
            return new RealtimeHolder<T>(obj, query);
        }

        public RealtimeHolder<T> AsRealtimeObject<T>(string path)
            where T : FirebaseObject
        {
            var obj = FirebaseObject.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<T>(obj.ParseModel<T>(), query);
        }

        public RealtimeHolder<FirebaseObject> AsRealtimeObject(FirebaseObject obj)
        {
            var query = new ChildQuery(App, this, () => obj.Key);
            return new RealtimeHolder<FirebaseObject>(obj, query);
        }

        public RealtimeHolder<FirebaseObject> AsRealtimeObject(string path)
        {
            var obj = FirebaseObject.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<FirebaseObject>(obj, query);
        }

        public RealtimeHolder<FirebasePropertyGroup> AsRealtimePropertyGroup(FirebasePropertyGroup group)
        {
            var query = new ChildQuery(App, this, () => group.Key);
            return new RealtimeHolder<FirebasePropertyGroup>(group, query);
        }

        public RealtimeHolder<FirebasePropertyGroup> AsRealtimePropertyGroup(string path)
        {
            var group = FirebasePropertyGroup.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<FirebasePropertyGroup>(group, query);
        }

        public RealtimeHolder<FirebaseObjectGroup> AsRealtimeObjectGroup(FirebaseObjectGroup group)
        {
            var query = new ChildQuery(App, this, () => group.Key);
            return new RealtimeHolder<FirebaseObjectGroup>(group, query);
        }

        public RealtimeHolder<FirebaseObjectGroup> AsRealtimeObjectGroup(string path)
        {
            var group = FirebaseObjectGroup.CreateFromKey(path);
            var query = new ChildQuery(App, this, () => path);
            return new RealtimeHolder<FirebaseObjectGroup>(group, query);
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
