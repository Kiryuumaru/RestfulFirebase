using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using RestfulFirebase.Http;
using System.Threading;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Utilities;
using Newtonsoft.Json;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using ObservableHelpers.Utilities;
using System.Linq;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// The base implementation for firebase query operations.
    /// </summary>
    public abstract class FirebaseQuery : Disposable, IFirebaseQuery
    {
        #region Properties

        /// <summary>
        /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
        /// </summary>
        public bool AuthenticateRequests { get; set; }

        /// <summary>
        /// The parent of the query.
        /// </summary>
        protected FirebaseQuery Parent { get; }

        private IHttpClientProxy client;

        #endregion

        #region Initializers

        private protected FirebaseQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        {
            App = app;
            Parent = parent;
            AuthenticateRequests = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the url segement of the query.
        /// </summary>
        /// <param name="child">
        /// The <see cref="FirebaseQuery"/> child of the created url.
        /// </param>
        /// <returns>
        /// The built url segement.
        /// </returns>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthTokenExpiredException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserDisabledException">
        /// The user account has been disabled by an administrator.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthInvalidRefreshTokenException">
        /// An invalid refresh token is provided.
        /// </exception>
        /// <exception cref="AuthInvalidJSONReceivedException">
        /// Invalid JSON payload received.
        /// </exception>
        /// <exception cref="AuthMissingRefreshTokenException">
        /// No refresh token provided.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        protected abstract string BuildUrlSegment(IFirebaseQuery child);

        /// <summary>
        /// Builds the url segement of the query.
        /// </summary>
        /// <param name="child">
        /// The <see cref="FirebaseQuery"/> child of the created url.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the built url segement.
        /// </returns>
        /// <exception cref="AuthAPIKeyNotValidException">
        /// API key not valid. Please pass a valid API key.
        /// </exception>
        /// <exception cref="AuthTokenExpiredException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthUserDisabledException">
        /// The user account has been disabled by an administrator.
        /// </exception>
        /// <exception cref="AuthUserNotFoundException">
        /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
        /// </exception>
        /// <exception cref="AuthInvalidIDTokenException">
        /// The user's credential is no longer valid. The user must sign in again.
        /// </exception>
        /// <exception cref="AuthInvalidRefreshTokenException">
        /// An invalid refresh token is provided.
        /// </exception>
        /// <exception cref="AuthInvalidJSONReceivedException">
        /// Invalid JSON payload received.
        /// </exception>
        /// <exception cref="AuthMissingRefreshTokenException">
        /// No refresh token provided.
        /// </exception>
        /// <exception cref="AuthUndefinedException">
        /// The error occured is undefined.
        /// </exception>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was cancelled.
        /// </exception>
        protected abstract Task<string> BuildUrlSegmentAsync(IFirebaseQuery child);

        internal AuthQuery WithAuth(Func<Task<string>> tokenFactory)
        {
            return new AuthQuery(App, this, tokenFactory);
        }

        internal SilentQuery Silent()
        {
            return new SilentQuery(App, this);
        }

        internal string BuildUrl(IFirebaseQuery child)
        {
            var url = BuildUrlSegment(child);

            if (Parent != null)
            {
                url = Parent.BuildUrl(this) + url;
            }

            return url;
        }

        private async Task<string> BuildUrlAsync(FirebaseQuery child)
        {
            var url = await BuildUrlSegmentAsync(child);

            if (Parent != null)
            {
                url = (await Parent.BuildUrlAsync(this)) + url;
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

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                client?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Object Members

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetAbsoluteUrl();
        }

        #endregion

        #region IFirebaseQuery Members

        /// <inheritdoc/>
        public RestfulFirebaseApp App { get; }

        /// <inheritdoc/>
        public RealtimeWire AsRealtimeWire(ILocalDatabase customLocalDatabase = default)
        {
            return new RealtimeWire(App, this, customLocalDatabase ?? App.Config.LocalDatabase);
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
                return await WithAuth(() => App.Auth.Session.GetFreshToken()).BuildUrlAsync((FirebaseQuery)null);
            }

            return await BuildUrlAsync((FirebaseQuery)null);
        }

        /// <inheritdoc/>
        public ChildQuery Child(Func<string> pathFactory)
        {
            if (pathFactory == null)
            {
                throw new ArgumentNullException(nameof(pathFactory));
            }
            return new ChildQuery(App, this, pathFactory);
        }

        /// <inheritdoc/>
        public ChildQuery Child(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            return Child(() => path);
        }

        /// <inheritdoc/>
        public async Task<bool> FanOut(Func<string> jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            if (relativePaths == null)
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }
            foreach (var path in relativePaths)
            {
                if (path.Any(
                    c =>
                    {
                        switch (c)
                        {
                            case '$': return true;
                            case '[': return true;
                            case ']': return true;
                            case '#': return true;
                            case '.': return true;
                            default:
                                if ((c >= 0 && c <= 31) || c == 127)
                                {
                                    return true;
                                }
                                return false;
                        }
                    }))
                {
                    throw new DatabaseForbiddenNodeNameCharacter();
                }
            }

            return await Patch(() =>
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
        public Task<bool> FanOut(string jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            return FanOut(() => jsonData, relativePaths, token, onException);
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

            while (true)
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
                        if (await retryEx.Retry.ConfigureAwait(false))
                        {
                            continue;
                        }
                    }
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public virtual string GetAbsoluteUrl()
        {
            return BuildUrl(this);
        }

        /// <inheritdoc/>
        public async Task<bool> Patch(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(string jsonToInvoke)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                var c = GetClient();

                if (jsonToInvoke == null)
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
                    await Silent().SendAsync(c, jsonToInvoke, new HttpMethod("PATCH"), token).ConfigureAwait(false);
                }
            };

            while (true)
            {
                try
                {
                    await invoke(jsonData()).ConfigureAwait(false);
                    return true;
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
                        if (await retryEx.Retry.ConfigureAwait(false))
                        {
                            continue;
                        }
                    }
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public Task<bool> Patch(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            return Patch(() => jsonData, token, onException);
        }

        /// <inheritdoc/>
        public async Task<bool> Put(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            async Task invoke(string jsonToInvoke)
            {
                string url;
                var responseData = string.Empty;
                var statusCode = HttpStatusCode.OK;

                if (App.Config.OfflineMode)
                {
                    throw new OfflineModeException();
                }

                var c = GetClient();

                if (jsonToInvoke == null)
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
                    await Silent().SendAsync(c, jsonToInvoke, HttpMethod.Put, token).ConfigureAwait(false);
                }
            };

            while (true)
            {
                try
                {
                    await invoke(jsonData()).ConfigureAwait(false);
                    return true;
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
                        if (await retryEx.Retry.ConfigureAwait(false))
                        {
                            continue;
                        }
                    }
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public Task<bool> Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null)
        {
            return Put(() => jsonData, token, onException);
        }

        #endregion
    }
}
