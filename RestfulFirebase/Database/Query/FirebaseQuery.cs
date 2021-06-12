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
using RestfulFirebase.Extensions;

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

        public ChildQuery Child(Func<string> pathFactory)
        {
            return new ChildQuery(App, this, pathFactory);
        }

        public ChildQuery Child(string path)
        {
            return Child(() => path);
        }

        public async Task Put(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(Func<string> invokeJsonData)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OfflineMode, new Exception("Offline mode"));
                }

                var c = GetClient();

                var currentJsonToInvoke = invokeJsonData();

                if (currentJsonToInvoke == null)
                {
                    try
                    {
                        url = await BuildUrlAsync(token).ConfigureAwait(false);

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
                        statusCode = result.StatusCode;
                        responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result.EnsureSuccessStatusCode();
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                    }
                    catch (FirebaseException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw new FirebaseException(ExceptionHelpers.GetFailureReason(statusCode), ex);
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

        public async Task Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            await Put(() => jsonData, token, onException).ConfigureAwait(false);
        }

        public async Task<string> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task<string> invoke()
            {
                var url = string.Empty;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OfflineMode, new Exception("Offline mode"));
                }

                try
                {
                    url = await BuildUrlAsync(token).ConfigureAwait(false);

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
                    statusCode = response.StatusCode;
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();
                    response.Dispose();

                    return responseData;
                }
                catch (OperationCanceledException ex)
                {
                    throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
                }
                catch (FirebaseException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new FirebaseException(ExceptionHelpers.GetFailureReason(statusCode), ex);
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

        public RealtimeWire AsRealtimeWire()
        {
            return new RealtimeWire(App, this);
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

            try
            {
                if (App.Auth.IsAuthenticated && AuthenticateRequests)
                {
                    return await Task.Run(delegate
                    {
                        return WithAuth(() =>
                        {
                            var getTokenResult = App.Auth.GetFreshToken();
                            if (!getTokenResult.Result.IsSuccess) throw getTokenResult.Result.Exception;
                            return getTokenResult.Result.Result;
                        }).BuildUrl(null);
                    }, token.Value).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
            }
            catch (FirebaseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new FirebaseException(FirebaseExceptionReason.DatabaseUndefined, ex);
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

        public override string ToString()
        {
            return GetAbsolutePath();
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
                statusCode = result.StatusCode;
                responseData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return responseData;
            }
            catch (OperationCanceledException ex)
            {
                throw new FirebaseException(FirebaseExceptionReason.OperationCancelled, ex);
            }
            catch (FirebaseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new FirebaseException(ExceptionHelpers.GetFailureReason(statusCode), ex);
            }
        }
    }
}
