using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using RestfulFirebase.RealtimeDatabase.Utilities;
using RestfulFirebase.RealtimeDatabase.Models;
using RestfulHelpers.Common;
using RestfulFirebase.RealtimeDatabase.Queries2;
using RestfulFirebase.RealtimeDatabase.Exceptions;
using RestfulFirebase.RealtimeDatabase.Enums;
using static System.Net.WebRequestMethods;
using RestfulHelpers;

namespace RestfulFirebase.RealtimeDatabase.Streaming;

internal class NodeStreamer : IDisposable
{
    #region Properties

    public FirebaseApp App { get; }

    private readonly CancellationTokenSource cancel;
    private readonly QueryRoot query;
    private readonly HttpClient http;
    private readonly string absoluteUrl;

    private readonly Action<StreamObject> onNext;
    private readonly Action<StreamError> onError;

    #endregion

    #region Initializers

    public NodeStreamer(
        FirebaseApp app,
        QueryRoot query,
        Action<StreamObject> onNext,
        Action<StreamError> onError,
        CancellationToken cancellationToken)
    {
        App = app;
        this.query = query;
        this.onNext = onNext;
        this.onError = onError;

        cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var httpClient = App.GetStreamHttpClient();

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        http = httpClient;
        absoluteUrl = query.Reference.Url;
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
        return CancellationTokenSource.CreateLinkedTokenSource(cancel.Token, new CancellationTokenSource(App.Config.DatabaseColdStreamTimeout).Token).Token;
    }

    private async void ReceiveThread()
    {
        while (true)
        {
            try
            {
                if (cancel.IsCancellationRequested) break;

                CancellationToken initToken = GetTrancientToken();

                var requestUrlResponse = await query.BuildUrl(initToken);
                if (requestUrlResponse.IsError)
                {
                    if (requestUrlResponse.Error is not OperationCanceledException)
                    {
                        onError?.Invoke(new StreamError(absoluteUrl, requestUrlResponse.Error));
                    }
                    continue;
                }

                string requestUrl = requestUrlResponse.Result;

                if (initToken.IsCancellationRequested) break;

                var streamResponse = await http.Execute(App.GetStreamHttpRequestMessage(HttpMethod.Get, requestUrl), HttpCompletionOption.ResponseHeadersRead, GetTrancientToken());
                if (streamResponse.IsError || streamResponse.HttpTransactions.FirstOrDefault() is not HttpTransaction httpTransaction)
                {
                    onError?.Invoke(new StreamError(absoluteUrl, await RealtimeDatabaseApi.GetHttpException(streamResponse)));
                    continue;
                }

                if (initToken.IsCancellationRequested) break;

                var line = string.Empty;

                try
                {
                    var serverEvent = ServerEventType.KeepAlive;

                    using var stream = httpTransaction.ResponseMessage == null ? null : await httpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
                    if (stream == null)
                    {
                        continue;
                    }

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
                            line = (await reader.ReadLineAsync(inStreamToken))?.Trim();
                        }
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
                    onError?.Invoke(new StreamError(absoluteUrl, ex));
                }

                if (cancel.IsCancellationRequested) break;
            }
            finally
            {
                await Task.Delay(App.Config.DatabaseRetryDelay);
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
                var path = pathToken.ToString().Trim('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                onNext?.Invoke(new StreamObject(dataToken, url, path));
                break;
            case ServerEventType.KeepAlive:
                break;
            case ServerEventType.Cancel:
                onError?.Invoke(new StreamError(url, new RealtimeDatabaseException(RealtimeDatabaseErrorType.UnauthorizedException, "Database stream is cancelled.", url, default, default, default, default)));
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

    #region IDisposable Members

    public void Dispose()
    {
        if (!cancel.IsCancellationRequested)
        {
            cancel.Cancel();
        }
    }

    #endregion
}
