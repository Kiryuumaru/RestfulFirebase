namespace RestfulFirebase.RealtimeDatabase.Query;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using RestfulFirebase.Http;
using System.Threading;
using RestfulFirebase.RealtimeDatabase.Realtime;
using RestfulFirebase.Utilities;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using System.Linq;
using DisposableHelpers;
using System.Text.Json;
using DisposableHelpers.Attributes;

/// <summary>
/// The base implementation for firebase query operations.
/// </summary>
[Disposable]
public abstract partial class FirebaseQuery : IFirebaseQuery
{
    #region Properties

    /// <summary>
    /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
    /// </summary>
    public bool AuthenticateRequests { get; set; } = true;

    /// <summary>
    /// The parent of the query.
    /// </summary>
    protected FirebaseQuery? Parent { get; }

    private HttpClient? client;

    #endregion

    #region Initializers

    private protected FirebaseQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery? parent)
    {
        App = realtimeDatabase.App;
        RealtimeDatabase = realtimeDatabase;
        Parent = parent;
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
    protected abstract Task<string> BuildUrlSegmentAsync(IFirebaseQuery? child);

    internal AuthQuery WithAuth()
    {
        return new AuthQuery(RealtimeDatabase, this);
    }

    internal SilentQuery Silent()
    {
        return new SilentQuery(RealtimeDatabase, this);
    }

    internal string BuildUrl(IFirebaseQuery child)
    {
        var url = BuildUrlSegment(child);

        if (Parent == null)
        {
            url = RealtimeDatabase.DatabaseUrl + url;
        }
        else
        {
            url = Parent.BuildUrl(this) + url;
        }

        return url;
    }

    private async Task<string> BuildUrlAsync(FirebaseQuery? child)
    {
        var url = await BuildUrlSegmentAsync(child);

        if (Parent == null)
        {
            url = RealtimeDatabase.DatabaseUrl + url;
        }
        else
        {
            url = (await Parent.BuildUrlAsync(this)) + url;
        }

        return url;
    }

    private HttpClient GetClient()
    {
        client ??= App.Config.HttpClientFactory.GetHttpClient();

        return client;
    }

    private async Task<string> SendAsync(HttpClient client, string data, HttpMethod method, CancellationToken? cancellationToken = null)
    {
        string responseData;
        var statusCode = HttpStatusCode.OK;
        var requestData = data;

        if (cancellationToken == null)
        {
            cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
        }

        string url;

        url = await BuildUrl(cancellationToken).ConfigureAwait(false);

        try
        {
            var message = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(requestData)
            };

            HttpResponseMessage result = await client.SendAsync(message, cancellationToken.Value).ConfigureAwait(false);
            cancellationToken.Value.ThrowIfCancellationRequested();
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

    /// <summary>
    /// The dispose logic.
    /// </summary>
    /// <param name = "disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            client?.Dispose();
        }
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
    public RealtimeDatabase RealtimeDatabase { get; }

    /// <inheritdoc/>
    public RealtimeModel<TModel> Subscribe<TModel>(TModel model, params string[] path)
        where TModel : notnull
    {
        VerifyNotDisposed();
        FirebasePathUtilities.EnsureValidPath(path);

        var wire = AsRealtimeWire();
        wire.Start();

        RealtimeModel<TModel> realtimeModel;

        if (path == null || path.Length == 0)
        {
            realtimeModel = wire.Subscribe(model);
        }
        else
        {
            realtimeModel = wire.Subscribe(model, path);
        }

        void RealtimeModel_Disposing(object sender, EventArgs e)
        {
            if (!wire.IsDisposedOrDisposing)
            {
                wire.Stop();
                wire.Dispose();
            }
        }

        realtimeModel.Disposing += RealtimeModel_Disposing;

        return realtimeModel;
    }

    /// <inheritdoc/>
    public RealtimeModel<TModel> WriteAndSubscribe<TModel>(TModel model, params string[] path)
        where TModel : notnull
    {
        VerifyNotDisposed();
        FirebasePathUtilities.EnsureValidPath(path);

        var wire = AsRealtimeWire();
        wire.Start();

        RealtimeModel<TModel> realtimeModel;

        if (path == null || path.Length == 0)
        {
            realtimeModel = wire.WriteAndSubscribe(model);
        }
        else
        {
            realtimeModel = wire.WriteAndSubscribe(model, path);
        }

        void RealtimeModel_Disposing(object sender, EventArgs e)
        {
            if (!wire.IsDisposedOrDisposing)
            {
                wire.Stop();
                wire.Dispose();
            }
        }

        realtimeModel.Disposing += RealtimeModel_Disposing;

        return realtimeModel;
    }

    /// <inheritdoc/>
    public RealtimeWire AsRealtimeWire()
    {
        return new RealtimeWire(App, this);
    }

    /// <inheritdoc/>
    public async Task<string> BuildUrl(CancellationToken? cancellationToken = null)
    {
        if (cancellationToken == null)
        {
            cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
        }

        if (App.Auth.Session != null && AuthenticateRequests)
        {
            return await Task.Run(() => WithAuth().BuildUrlAsync((FirebaseQuery?)null), cancellationToken.Value);
        }

        return await Task.Run(() => BuildUrlAsync((FirebaseQuery?)null), cancellationToken.Value);
    }

    /// <inheritdoc/>
    public ChildQuery Child(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        return new ChildQuery(RealtimeDatabase, this, path);
    }

    /// <inheritdoc/>
    public async Task<bool> FanOut(IDictionary<string, string> pathValues, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        if (pathValues == null)
        {
            throw new ArgumentNullException(nameof(pathValues));
        }
        foreach (var path in pathValues.Values)
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
            var fanoutObject = new Dictionary<string, object?>(pathValues.Count);

            foreach (var pathValue in pathValues)
            {
                fanoutObject.Add(pathValue.Key, pathValue.Value == null ? null : JsonSerializer.Deserialize<string>(pathValue.Value, RestfulFirebaseApp.DefaultJsonSerializerOption));
            }

            return JsonSerializer.Serialize(fanoutObject);
        }, cancellationToken, onException);
    }

    /// <inheritdoc/>
    public async Task<bool> FanOut(Func<string?> jsonData, string[] relativePaths, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
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
            var fanoutObject = new Dictionary<string, object?>(relativePaths.Length);

            var json = jsonData();

            foreach (var path in relativePaths)
            {
                fanoutObject.Add(path, json == null ? null : JsonSerializer.Deserialize<string>(json, RestfulFirebaseApp.DefaultJsonSerializerOption));
            }

            return JsonSerializer.Serialize(fanoutObject);
        }, cancellationToken, onException);
    }

    /// <inheritdoc/>
    public Task<bool> FanOut(string? jsonData, string[] relativePaths, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        return FanOut(() => jsonData, relativePaths, cancellationToken, onException);
    }

    /// <inheritdoc/>
    public async Task<string?> Get(CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
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

            url = await BuildUrl(cancellationToken).ConfigureAwait(false);

            try
            {
                if (cancellationToken == null)
                {
                    cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
                }

                var response = await GetClient().GetAsync(url, cancellationToken.Value).ConfigureAwait(false);
                cancellationToken.Value.ThrowIfCancellationRequested();
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

/* Unmerged change from project 'RestfulFirebase (net6.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/

/* Unmerged change from project 'RestfulFirebase (net5.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>>)async delegate
                {
                    await Task.Delay((TimeSpan)App.Config.DatabaseRetryDelay).ConfigureAwait(false);
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
    public async Task<bool> Patch(Func<string?> jsonData, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        async Task invoke(string? jsonToInvoke)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            if (App.Config.OfflineMode)
            {
                throw new OfflineModeException();
            }

            var c = GetClient();

            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }

            if (jsonToInvoke == null)
            {
                url = await BuildUrl(cancellationToken).ConfigureAwait(false);

                try
                {
                    var result = await c.DeleteAsync(url, cancellationToken.Value).ConfigureAwait(false);
                    cancellationToken.Value.ThrowIfCancellationRequested();
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
                await Silent().SendAsync(c, jsonToInvoke, new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
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

/* Unmerged change from project 'RestfulFirebase (net6.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/

/* Unmerged change from project 'RestfulFirebase (net5.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>>)async delegate
                {
                    await Task.Delay((TimeSpan)App.Config.DatabaseRetryDelay).ConfigureAwait(false);
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
    public Task<bool> Patch(string? jsonData, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        return Patch(() => jsonData, cancellationToken, onException);
    }

    /// <inheritdoc/>
    public async Task<bool> Put(Func<string?> jsonData, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        async Task invoke(string? jsonToInvoke)
        {
            string url;
            var responseData = string.Empty;
            var statusCode = HttpStatusCode.OK;

            if (App.Config.OfflineMode)
            {
                throw new OfflineModeException();
            }

            var c = GetClient();

            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }

            if (jsonToInvoke == null)
            {
                url = await BuildUrl(cancellationToken).ConfigureAwait(false);

                try
                {
                    var result = await c.DeleteAsync(url, cancellationToken.Value).ConfigureAwait(false);
                    cancellationToken.Value.ThrowIfCancellationRequested();
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
                await Silent().SendAsync(c, jsonToInvoke, HttpMethod.Put, cancellationToken).ConfigureAwait(false);
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

/* Unmerged change from project 'RestfulFirebase (net6.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/

/* Unmerged change from project 'RestfulFirebase (net5.0)'
Before:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run(async delegate
After:
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>?>)async delegate
*/
                var retryEx = new RetryExceptionEventArgs(ex, Task.Run((Func<Task<bool>>)async delegate
                {
                    await Task.Delay((TimeSpan)App.Config.DatabaseRetryDelay).ConfigureAwait(false);
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
    public Task<bool> Put(string? jsonData, CancellationToken? cancellationToken = null, Action<RetryExceptionEventArgs>? onException = null)
    {
        return Put(() => jsonData, cancellationToken, onException);
    }

    #endregion
}
