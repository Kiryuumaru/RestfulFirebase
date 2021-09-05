using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RestfulFirebase.Database.Streaming;
using System.Net;
using RestfulFirebase.Http;
using System.Linq;
using RestfulFirebase.Database.Models;
using System.IO;
using System.Threading;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Realtime;
using System.Collections.Concurrent;
using RestfulFirebase.Utilities;
using Newtonsoft.Json;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// The base implementation for firebase query operations.
    /// </summary>
    public abstract class FirebaseQuery : IFirebaseQuery, IDisposable
    {
        private IHttpClientProxy client;

        /// <summary>
        /// The parent of the query.
        /// </summary>
        protected FirebaseQuery Parent { get; }

        private protected FirebaseQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        {
            App = app;
            Parent = parent;
            AuthenticateRequests = true;
        }

        /// <summary>
        /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
        /// </summary>
        public bool AuthenticateRequests { get; set; }

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; }

        /// <inheritdoc/>
        public ChildQuery Child(Func<string> pathFactory)
        {
            return new ChildQuery(App, this, pathFactory);
        }

        /// <inheritdoc/>
        public ChildQuery Child(string path)
        {
            return Child(() => path);
        }

        /// <inheritdoc/>
        public async Task Put(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(Func<string> invokeJsonData)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                var c = GetClient();

                var currentJsonToInvoke = invokeJsonData();

                if (currentJsonToInvoke == null)
                {
                    url = await BuildUrl(token).ConfigureAwait(false);

                    try
                    {
                        CancellationToken invokeToken;

                        if (token == null)
                        {
                            invokeToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                        }
                        else
                        {
                            invokeToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                        }

                        var result = await c.DeleteAsync(url, invokeToken).ConfigureAwait(false);
                        invokeToken.ThrowIfCancellationRequested();
                        statusCode = result.StatusCode;
                        responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result.EnsureSuccessStatusCode();
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw ExceptionHelpers.GetException(statusCode, ex);
                    }
                }
                else
                {
                    await Silent().SendAsync(c, currentJsonToInvoke, HttpMethod.Put, token).ConfigureAwait(false);
                }
            };

            async Task recursive()
            {
                try
                {
                    await invoke(jsonData).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
                    {
                        await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                        return false;
                    }));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry != null)
                    {
                        if (await retryEx.Retry)
                        {
                            await recursive().ConfigureAwait(false);
                        }
                    }
                }
            }

            await recursive().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            await Put(() => jsonData, token, onException).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Patch(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(Func<string> invokeJsonData)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                var c = GetClient();

                var currentJsonToInvoke = invokeJsonData();

                if (currentJsonToInvoke == null)
                {
                    url = await BuildUrl(token).ConfigureAwait(false);

                    try
                    {
                        CancellationToken invokeToken;

                        if (token == null)
                        {
                            invokeToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                        }
                        else
                        {
                            invokeToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                        }

                        var result = await c.DeleteAsync(url, invokeToken).ConfigureAwait(false);
                        invokeToken.ThrowIfCancellationRequested();
                        statusCode = result.StatusCode;
                        responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result.EnsureSuccessStatusCode();
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw ExceptionHelpers.GetException(statusCode, ex);
                    }
                }
                else
                {
                    await Silent().SendAsync(c, currentJsonToInvoke, new HttpMethod("PATCH"), token).ConfigureAwait(false);
                }
            };

            async Task recursive()
            {
                try
                {
                    await invoke(jsonData).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
                    {
                        await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                        return false;
                    }));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry != null)
                    {
                        if (await retryEx.Retry)
                        {
                            await recursive().ConfigureAwait(false);
                        }
                    }
                }
            }

            await recursive().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Patch(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            await Patch(() => jsonData, token, onException).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task FanOut(Func<string> jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            if (relativePaths == null)
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }

            await Patch(() =>
            {
                var fanoutObject = new Dictionary<string, object>(relativePaths.Length);

                var json = jsonData();

                foreach (var path in relativePaths)
                {
                    fanoutObject.Add(path, JsonConvert.DeserializeObject(json));
                }

                return JsonConvert.SerializeObject(fanoutObject);
            }, token, onException);
        }

        /// <inheritdoc/>
        public async Task FanOut(string jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            await FanOut(() => jsonData, relativePaths, token, onException);
        }

        /// <inheritdoc/>
        public async Task<string> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task<string> invoke()
            {
                var url = string.Empty;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                url = await BuildUrl(token).ConfigureAwait(false);

                try
                {
                    CancellationToken invokeToken;

                    if (token == null)
                    {
                        invokeToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                    }
                    else
                    {
                        invokeToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                    }

                    var response = await GetClient().GetAsync(url, invokeToken).ConfigureAwait(false);
                    invokeToken.ThrowIfCancellationRequested();
                    statusCode = response.StatusCode;
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                    response.Dispose();

                    return responseData;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw ExceptionHelpers.GetException(statusCode, ex);
                }
            }

            async Task<string> recursive()
            {
                try
                {
                    return await invoke().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
                    {
                        await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                        return false;
                    }));
                    onException?.Invoke(retryEx);
                    if (retryEx.Retry != null)
                    {
                        if (await retryEx.Retry)
                        {
                            await recursive().ConfigureAwait(false);
                        }
                    }
                    return null;
                }
            }

            return await recursive().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public RealtimeWire AsRealtimeWire(ILocalDatabase customLocalDatabase = default)
        {
            var wire = new RealtimeWire(App, this, customLocalDatabase ?? App.Config.LocalDatabase);
            wire.EvaluateData();
            return wire;
        }

        /// <inheritdoc/>
        public async Task<string> BuildUrl(CancellationToken? token = null)
        {
            if (token == null)
            {
                token = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }
            else
            {
                token = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
            }

            if (App.Auth.IsAuthenticated && AuthenticateRequests)
            {
                return await Task.Run(delegate
                {
                    return WithAuth(() => App.Auth.Session.GetFreshToken().Result).BuildUrl((FirebaseQuery)null);
                }, token.Value).ConfigureAwait(false);
            }

            return BuildUrl((FirebaseQuery)null);
        }

        /// <inheritdoc/>
        public string GetAbsolutePath()
        {
            var url = BuildUrlSegment(this);

            if (Parent != null)
            {
                url = Parent.BuildUrl(this) + url;
            }

            return url;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            client?.Dispose();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetAbsolutePath();
        }

        /// <inheritdoc/>
        protected abstract string BuildUrlSegment(FirebaseQuery child);

        internal AuthQuery WithAuth(Func<string> tokenFactory)
        {
            return new AuthQuery(App, this, tokenFactory);
        }

        internal SilentQuery Silent()
        {
            return new SilentQuery(App, this);
        }

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

            url = await BuildUrl(token).ConfigureAwait(false);

            try
            {
                var message = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(requestData)
                };

                CancellationToken invokeToken;
                if (token == null)
                {
                    invokeToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                }
                else
                {
                    invokeToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token).Token;
                }

                HttpResponseMessage result = null;
                result = await client.SendAsync(message, invokeToken).ConfigureAwait(false);
                invokeToken.ThrowIfCancellationRequested();
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return responseData;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ExceptionHelpers.GetException(statusCode, ex);
            }
        }
    }
}
