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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestfulFirebase.Exceptions;

namespace RestfulFirebase.Database.Streaming
{
    internal class NodeStreamer : IDisposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        private readonly CancellationTokenSource cancel;
        private readonly IFirebaseQuery query;
        private readonly HttpClient http;

        private EventHandler<StreamObject> onNext;
        private EventHandler<Exception> onError;

        #endregion

        #region Initializers

        public NodeStreamer(
            RestfulFirebaseApp app,
            IFirebaseQuery query,
            EventHandler<StreamObject> onNext,
            EventHandler<Exception> onError)
        {
            App = app;
            this.query = query;
            this.onNext = onNext;
            this.onError = onError;

            cancel = new CancellationTokenSource();

            var httpClient = App.Config.HttpStreamFactory.GetHttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            http = httpClient;
        }

        #endregion

        private CancellationToken GetTrancientToken()
        {
            return CancellationTokenSource.CreateLinkedTokenSource(cancel.Token, new CancellationTokenSource(App.Config.DatabaseColdStreamTimeout).Token).Token;
        }

        private async void ReceiveThread()
        {
            while (true)
            {
                bool isReady = true;

                if (cancel.IsCancellationRequested) break;

                string url = string.Empty;

                if (isReady)
                {
                    try
                    {
                        url = await query.BuildUrl().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(this, ex);
                        isReady = false;
                    }
                }

                if (cancel.IsCancellationRequested) break;

                var statusCode = HttpStatusCode.OK;
                HttpResponseMessage response = null;

                if (isReady)
                {
                    try
                    {
                        HttpRequestMessage request = App.Config.HttpStreamFactory.GetStreamHttpRequestMessage(HttpMethod.Get, url);
                        response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, GetTrancientToken()).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        isReady = false;
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(this, ExceptionHelpers.GetException(statusCode, ex));
                        isReady = false;
                    }
                }

                if (cancel.IsCancellationRequested) break;

                var line = string.Empty;

                if (isReady)
                {
                    try
                    {
                        var serverEvent = ServerEventType.KeepAlive;

                        statusCode = response.StatusCode;
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var reader = new NonBlockingStreamReader(stream))
                        {
                            try
                            {
                                reader.Peek(); // ReadlineAsync bug fix (no idea)
                            }
                            catch { }
                            while (true)
                            {
                                if (cancel.IsCancellationRequested) break;

                                line = string.Empty;

                                try
                                {
                                    line = (await reader.ReadLineAsync(GetTrancientToken()).ConfigureAwait(false))?.Trim();
                                }
                                catch (OperationCanceledException)
                                {
                                    break;
                                }
                                catch { }

                                if (cancel.IsCancellationRequested) break;

                                if (string.IsNullOrWhiteSpace(line))
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
                                        ProcessServerData(url, serverEvent, tuple[1]);
                                        break;
                                }

                                if (serverEvent == ServerEventType.AuthRevoked)
                                {
                                    // auth token no longer valid, reconnect
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(this, ExceptionHelpers.GetException(statusCode, ex));
                    }
                }

                if (cancel.IsCancellationRequested) break;

                await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
            }
        }

        private ServerEventType ParseServerEvent(ServerEventType serverEvent, string eventName)
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

        private void ProcessServerData(string url, ServerEventType serverEvent, string serverData)
        {
            switch (serverEvent)
            {
                case ServerEventType.Put:
                case ServerEventType.Patch:
                    var result = JObject.Parse(serverData);
                    var pathToken = result["path"];
                    var dataToken = result["data"];
                    var streamPath = pathToken.ToString();
                    var absolutePath = query.GetAbsolutePath();

                    var uri = streamPath == "/" ?
                        absolutePath : UrlUtilities.Combine(absolutePath, streamPath.Substring(1));

                    var type = dataToken.Type;

                    onNext?.Invoke(this, new StreamObject(Convert(dataToken), uri));

                    break;
                case ServerEventType.KeepAlive:
                    break;
                case ServerEventType.Cancel:
                    onError?.Invoke(this, new DatabaseUnauthorizedException());
                    break;
            }
        }

        private StreamData Convert(JToken token)
        {
            if (token.Type == JTokenType.Property ||
                token.Type == JTokenType.Comment ||
                token.Type == JTokenType.Undefined)
            {
                throw new Exception("Unknown stream data type");
            }
            else if (token.Type == JTokenType.Object)
            {
                var datas = JsonConvert.DeserializeObject<Dictionary<string, object>>(token.ToString());
                var subDatas = new Dictionary<string, StreamData>();
                foreach (var entry in datas)
                {
                    subDatas.Add(entry.Key, Convert(JToken.FromObject(entry.Value)));
                }
                return new MultiStreamData(subDatas);
            }
            else if (token.Type == JTokenType.Array)
            {
                var datas = JsonConvert.DeserializeObject<List<object>>(token.ToString());
                var subDatas = new Dictionary<string, StreamData>();
                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i] == null) subDatas.Add(i.ToString(), null);
                    else subDatas.Add(i.ToString(), Convert(JToken.FromObject(datas[i])));
                }
                return new MultiStreamData(subDatas);
            }
            else if (token.Type != JTokenType.Null)
            {
                return new SingleStreamData(((JValue)token).Value?.ToString());
            }
            else if (token.Type == JTokenType.Null)
            {
                return null;
            }
            else
            {
                throw new Exception("Unknown stream data type");
            }
        }

        public IDisposable Run()
        {
            Task.Run(ReceiveThread);
            return this;
        }

        public void Dispose()
        {
            cancel.Cancel();
        }
    }
}
