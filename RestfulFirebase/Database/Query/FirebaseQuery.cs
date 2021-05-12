using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RestfulFirebase.Database.Streaming;
using System.Net;
using RestfulFirebase.Extensions.Http;
using System.Linq;
using RestfulFirebase.Database.Models;
using System.IO;
using System.Threading;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Realtime;
using System.Collections.Concurrent;
using Newtonsoft.Json;

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

        public async Task Put(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(Func<string> invokeJsonData, CancellationToken? invokeToken)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                url = await BuildUrlAsync(invokeToken).ConfigureAwait(false);

                var c = GetClient();

                if (invokeJsonData == null)
                {
                    try
                    {
                        var result = await c.DeleteAsync(url, invokeToken.Value).ConfigureAwait(false);
                        statusCode = result.StatusCode;
                        responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        throw new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex);
                    }
                }
                else
                {
                    await Silent().SendAsync(c, invokeJsonData(), HttpMethod.Put, invokeToken);
                }
            };

            async Task recursive()
            {
                try
                {
                    CancellationToken recursiveToken;

                    if (token == null)
                    {
                        recursiveToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                    }
                    else
                    {
                        recursiveToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                    }

                    await invoke(jsonData, recursiveToken);
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Task.Delay(App.Config.DatabaseRetryDelay);
                        await recursive();
                    }
                }
            }

            await recursive();
        }

        public async Task Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            await Put(() => jsonData, token, onException);
        }

        public async Task<string> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task<string> invoke(CancellationToken? invokeToken)
            {
                var url = string.Empty;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                url = await this.BuildUrlAsync().ConfigureAwait(false);

                try
                {
                    var response = await GetClient().GetAsync(url).ConfigureAwait(false);
                    statusCode = response.StatusCode;
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                    response.Dispose();

                    return responseData;
                }
                catch (Exception ex)
                {
                    throw new FirebaseDatabaseException(url, string.Empty, responseData, statusCode, ex);
                }
            }

            async Task<string> recursive()
            {
                try
                {
                    CancellationToken recursiveToken;

                    if (token == null)
                    {
                        recursiveToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                    }
                    else
                    {
                        recursiveToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                    }

                    return await invoke(recursiveToken);
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs(ex);
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry)
                    {
                        await Task.Delay(App.Config.DatabaseRetryDelay);
                        await recursive();
                    }
                    return null;
                }
            }

            return await recursive();
        }

        public RealtimeWire<T> PutAsRealtime<T>(T model) where T : IRealtimeModel
        {
            return RealtimeWire<T>.CreateFromQuery(App, this, model, true);
        }

        public RealtimeWire<T> SubAsRealtime<T>(T model) where T : IRealtimeModel
        {
            return RealtimeWire<T>.CreateFromQuery(App, this, model, false);
        }

        public async Task<string> BuildUrlAsync(CancellationToken? token = null)
        {
            if (token == null)
            {
                token = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }
            else
            {
                token = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
            }

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
                }, token.Value);
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
            if (token == null)
            {
                token = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }
            else
            {
                token = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
            }

            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;
            var requestData = data;

            string url;

            try
            {
                url = await BuildUrlAsync(token).ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch (HttpRequestException ex)
            {
                throw ex;
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
                result = await client.SendAsync(message, token.Value).ConfigureAwait(false);
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return responseData;
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Type s = ex.GetType();
                throw new FirebaseDatabaseException(url, requestData, responseData, statusCode, ex);
            }
        }
    }
}
