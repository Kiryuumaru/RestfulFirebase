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
using System.Threading;
using RestfulFirebase.Auth;

namespace RestfulFirebase.Database.Query
{
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        private IHttpClientProxy client;

        protected readonly TimeSpan DefaultHttpClientTimeout = new TimeSpan(0, 0, 180);

        protected readonly FirebaseQuery Parent;

        protected FirebaseQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        {
            App = app;
            Parent = parent;
            AuthenticateRequests = true;
        }

        internal AuthQuery WithAuth(Func<string> tokenFactory)
        {
            return new AuthQuery(App, this, tokenFactory);
        }

        internal SilentQuery Silent()
        {
            return new SilentQuery(App, this);
        }

        public bool AuthenticateRequests { get; set; }

        public RestfulFirebaseApp App { get; }

        public async Task Put(string jsonData, TimeSpan? timeout = null, Action<RetryExceptionEventArgs<FirebaseDatabaseException>> onException = null)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                if (App.Auth.Authenticated && AuthenticateRequests)
                {
                    var refresh =  await App.Auth.RefreshAuthAsync(timeout);
                    if (!refresh.IsSuccess)
                    {
                        await Task.Delay(2000);
                        throw refresh.Exception;
                    }
                }
                url = await BuildUrlAsync().ConfigureAwait(false);
            }
            catch (FirebaseDatabaseException ex)
            {
                var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                onException?.Invoke(retryEx);
                if (retryEx.Retry)
                {
                    await Put(jsonData, timeout, onException);
                }
                return;
            }
            catch (Exception ex)
            {
                var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex));
                onException?.Invoke(retryEx);
                if (retryEx.Retry)
                {
                    await Put(jsonData, timeout, onException);
                }
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
                catch (FirebaseDatabaseException ex)
                {
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, timeout, onException);
                    }
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, timeout, onException);
                    }
                }
            }
            else
            {
                try
                {
                    var c = GetClient(timeout);
                    await Silent().SendAsync(c, jsonData, HttpMethod.Put);
                }
                catch (FirebaseDatabaseException ex)
                {
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, timeout, onException);
                    }
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, timeout, onException);
                    }
                }
            }
        }

        public RealtimeWire<T> AsRealtime<T>(T model) where T : IRealtimeModel
        {
            return new RealtimeWire<T>(model, this);
        }

        public async Task<string> BuildUrlAsync()
        {
            if (App.Auth.Authenticated && AuthenticateRequests)
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
                throw new FirebaseDatabaseException("Couldn't build the url", requestData, responseData, statusCode, ex);
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
                throw new FirebaseDatabaseException(url, requestData, responseData, statusCode, ex);
            }
        }
    }
}
