using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Utilities;
using RestfulFirebase.Exceptions;
using ObservableHelpers.Utilities;
using DisposableHelpers;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace RestfulFirebase.Database.Streaming;

internal class NodeStreamer : Disposable
{
    #region Properties

    public RestfulFirebaseApp App { get; }

    private readonly CancellationTokenSource cancel;
    private readonly IFirebaseQuery query;
    private readonly HttpClient http;

    private readonly EventHandler<StreamObject> onNext;
    private readonly EventHandler<ErrorEventArgs> onError;

    #endregion

    #region Initializers

    public NodeStreamer(
        RestfulFirebaseApp app,
        IFirebaseQuery query,
        EventHandler<StreamObject> onNext,
        EventHandler<ErrorEventArgs> onError)
    {
        App = app;
        this.query = query;
        this.onNext = onNext;
        this.onError = onError;

        cancel = new CancellationTokenSource();

        var httpClient = App.Config.CachedHttpStreamFactory.GetHttpClient();

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        http = httpClient;
    }

    #endregion

    #region Methods

    public IDisposable Run()
    {
        Task.Run(ReceiveThread);
        return this;
    }

    private CancellationToken GetTrancientToken()
    {
        return CancellationTokenSource.CreateLinkedTokenSource(cancel.Token, new CancellationTokenSource(App.Config.CachedDatabaseColdStreamTimeout).Token).Token;
    }

    private async void ReceiveThread()
    {
        while (true)
        {
            try
            {
                if (cancel.IsCancellationRequested) break;

                CancellationToken initToken = GetTrancientToken();

                string requestUrl = string.Empty;
                string absoluteUrl = query.GetAbsoluteUrl();

                try
                {
                    requestUrl = await query.BuildUrl(initToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(this, new ErrorEventArgs(absoluteUrl, ex));
                    continue;
                }

                if (initToken.IsCancellationRequested) break;

                HttpStatusCode statusCode = HttpStatusCode.OK;
                HttpResponseMessage? response = null;

                try
                {
                    HttpRequestMessage request = App.Config.CachedHttpStreamFactory.GetStreamHttpRequestMessage(HttpMethod.Get, requestUrl);
                    response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, GetTrancientToken()).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(this, new ErrorEventArgs(absoluteUrl, ExceptionHelpers.GetException(statusCode, ex)));
                    continue;
                }

                if (initToken.IsCancellationRequested) break;

                var line = string.Empty;

                try
                {
                    var serverEvent = ServerEventType.KeepAlive;

                    statusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    using var reader = new NonBlockingStreamReader(stream);
                    try
                    {
                        reader.Peek(); // ReadlineAsync bug fix (no idea)
                    }
                    catch { }
                    while (true)
                    {
                        CancellationToken inStreamToken = GetTrancientToken();

                        if (inStreamToken.IsCancellationRequested) break;

                        line = string.Empty;

                        try
                        {
                            line = (await reader.ReadLineAsync(inStreamToken).ConfigureAwait(false))?.Trim();
                        }
                        //catch (OperationCanceledException)
                        //{
                        //    break;
                        //}
                        catch
                        {
                            break;
                        }

                        if (inStreamToken.IsCancellationRequested) break;

                        if (line == null || string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        var tuple = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                        switch (tuple[0].ToLower())
                        {
                            case "event":
                                serverEvent = ParseServerEvent(serverEvent, tuple[1]);
                                break;
                            case "data":
                                ProcessServerData(absoluteUrl, serverEvent, tuple[1]);
                                break;
                        }

                        if (serverEvent == ServerEventType.AuthRevoked)
                        {
                            // auth token no longer valid, reconnect
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(this, new ErrorEventArgs(absoluteUrl, ExceptionHelpers.GetException(statusCode, ex)));
                }

                if (cancel.IsCancellationRequested) break;
            }
            finally
            {
                await Task.Delay(App.Config.CachedDatabaseRetryDelay).ConfigureAwait(false);
            }
        }
    }

    private void ProcessServerData(string url, ServerEventType serverEvent, string serverData)
    {
        switch (serverEvent)
        {
            case ServerEventType.Put:
            case ServerEventType.Patch:
                var result = JsonDocument.Parse(serverData);
                var pathToken = result.RootElement.GetProperty("path");
                var dataToken = result.RootElement.GetProperty("data");
                var path = pathToken.ToString();
                onNext?.Invoke(this, new StreamObject(dataToken, url, path[1..]));
                break;
            case ServerEventType.KeepAlive:
                break;
            case ServerEventType.Cancel:
                onError?.Invoke(this, new ErrorEventArgs(url, new DatabaseUnauthorizedException()));
                break;
        }
    }

    private static ServerEventType ParseServerEvent(ServerEventType serverEvent, string eventName)
    {
        switch (eventName)
        {
            case "put":
                serverEvent = ServerEventType.Put;
                break;
            case "patch":
                serverEvent = ServerEventType.Patch;
                break;
            case "keep-alive":
                serverEvent = ServerEventType.KeepAlive;
                break;
            case "cancel":
                serverEvent = ServerEventType.Cancel;
                break;
            case "auth_revoked":
                serverEvent = ServerEventType.AuthRevoked;
                break;
        }

        return serverEvent;
    }

    #endregion

    #region Disposable Members

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancel.Cancel();
        }
        base.Dispose(disposing);
    }

    #endregion
}
