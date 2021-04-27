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

        public async Task Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs<FirebaseDatabaseException>> onException = null)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            try
            {
                url = await BuildUrlAsync(token).ConfigureAwait(false);
            }
            catch (FirebaseDatabaseException ex)
            {
                await Task.Delay(2000);
                var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                onException?.Invoke(retryEx);
                if (retryEx.Retry)
                {
                    await Put(jsonData, token, onException);
                }
                return;
            }
            catch (Exception ex)
            {
                await Task.Delay(2000);
                var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException("Couldn't build the url", string.Empty, responseData, statusCode, ex));
                onException?.Invoke(retryEx);
                if (retryEx.Retry)
                {
                    await Put(jsonData, token, onException);
                }
                return;
            }

            if (jsonData == null)
            {
                var c = GetClient();

                try
                {
                    HttpResponseMessage result = null;
                    if (token == null)
                    {
                        result = await c.DeleteAsync(url).ConfigureAwait(false);
                    }
                    else
                    {
                        result = await c.DeleteAsync(url, token.Value).ConfigureAwait(false);
                    }
                    statusCode = result.StatusCode;
                    responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                    result.EnsureSuccessStatusCode();
                }
                catch (FirebaseDatabaseException ex)
                {
                    await Task.Delay(2000);
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, token, onException);
                    }
                }
                catch (Exception ex)
                {
                    await Task.Delay(2000);
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, token, onException);
                    }
                }
            }
            else
            {
                try
                {
                    var c = GetClient();
                    await Silent().SendAsync(c, jsonData, HttpMethod.Put, token);
                }
                catch (FirebaseDatabaseException ex)
                {
                    await Task.Delay(2000);
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, token, onException);
                    }
                }
                catch (Exception ex)
                {
                    await Task.Delay(2000);
                    var retryEx = new RetryExceptionEventArgs<FirebaseDatabaseException>(new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Put(jsonData, token, onException);
                    }
                }
            }
        }

        public RealtimeWire<T> AsRealtime<T>(T model) where T : IRealtimeModel
        {
            return new RealtimeWire<T>(model, this);
        }

        public async Task<string> BuildUrlAsync(CancellationToken? token = null)
        {
            if (App.Auth.Authenticated && AuthenticateRequests)
            {
                string run()
                {
                    return WithAuth(() =>
                    {
                        var getTokenResult = App.Auth.GetFreshTokenAsync();
                        if (!getTokenResult.Result.IsSuccess) throw getTokenResult.Result.Exception;
                        return getTokenResult.Result.Result;
                    }).BuildUrl(null);
                };
                if (token == null)
                {
                    return await Task.Run(run);
                }
                else
                {
                    return await Task.Run(run, token.Value);
                }
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

        private HttpClient GetClient()
        {
            if (client == null)
            {
                client = App.Config.HttpClientFactory.GetHttpClient(App.Config.DatabaseRequestTimeout);
            }

            return client.GetHttpClient();
        }

        private async Task<string> SendAsync(HttpClient client, string data, HttpMethod method, CancellationToken? token = null)
        {
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;
            var requestData = data;
            string url;

            try
            {
                url = await BuildUrlAsync(token).ConfigureAwait(false);
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
                HttpResponseMessage result = null;
                if (token == null)
                {
                    result = await client.SendAsync(message).ConfigureAwait(false);
                }
                else
                {
                    result = await client.SendAsync(message, token.Value).ConfigureAwait(false);
                }
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
