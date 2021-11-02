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
using ObservableHelpers.Utilities;

namespace RestfulFirebase.Database.Streaming
{
    internal class NodeStreamer : Disposable
    {
        #region Properties

        public RestfulFirebaseApp App { get; }

        private readonly CancellationTokenSource cancel;
        private readonly IFirebaseQuery query;
        private readonly HttpClient http;

        private EventHandler<StreamObject> onNext;
        private EventHandler<ErrorEventArgs> onError;

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

            var httpClient = App.Config.HttpStreamFactory.GetHttpClient();

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
            return CancellationTokenSource.CreateLinkedTokenSource(cancel.Token, new CancellationTokenSource(App.Config.DatabaseColdStreamTimeout).Token).Token;
        }

        private async void ReceiveThread()
        {
            while (true)
            {
                try
                {
                    if (cancel.IsCancellationRequested) break;

                    string requestUrl = string.Empty;
                    string absoluteUrl = query.GetAbsoluteUrl();

                    try
                    {
                        requestUrl = await query.BuildUrl().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(this, new ErrorEventArgs(absoluteUrl, ex));
                        continue;
                    }

                    if (cancel.IsCancellationRequested) break;

                    var statusCode = HttpStatusCode.OK;
                    HttpResponseMessage response = null;

                    try
                    {
                        HttpRequestMessage request = App.Config.HttpStreamFactory.GetStreamHttpRequestMessage(HttpMethod.Get, requestUrl);
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

                    if (cancel.IsCancellationRequested) break;

                    var line = string.Empty;

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
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(this, new ErrorEventArgs(absoluteUrl, ExceptionHelpers.GetException(statusCode, ex)));
                    }

                    if (cancel.IsCancellationRequested) break;
                }
                finally
                {
                    await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                }
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
                    var dataToken = result["data"];
                    var streamPath = result["path"].ToString();
                    onNext?.Invoke(this, new StreamObject(Convert(dataToken), url, streamPath));
                    break;
                case ServerEventType.KeepAlive:
                    break;
                case ServerEventType.Cancel:
                    onError?.Invoke(this, new ErrorEventArgs(url, new DatabaseUnauthorizedException()));
                    break;
            }
        }

        private StreamData Convert(JToken token)
        {
            if (token.Type == JTokenType.Property ||
                token.Type == JTokenType.Comment ||
                token.Type == JTokenType.Undefined)
            {
                throw new DatabaseUndefinedException("Unknown stream data type");
            }
            else if (token.Type == JTokenType.Object)
            {
                var subDatas = new Dictionary<string, StreamData>();
                foreach (var entry in token as JObject)
                {
                    subDatas.Add(entry.Key, Convert(entry.Value));
                }
                return new MultiStreamData(subDatas);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray arrToken = token as JArray;
                var subDatas = new Dictionary<string, StreamData>();
                for (int i = 0; i < arrToken.Count; i++)
                {
                    subDatas.Add(i.ToString(), Convert(arrToken[i]));
                }
                return new MultiStreamData(subDatas);
            }
            else if (token.Type != JTokenType.Null)
            {
                return new SingleStreamData((token as JValue).ToString());
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
}
