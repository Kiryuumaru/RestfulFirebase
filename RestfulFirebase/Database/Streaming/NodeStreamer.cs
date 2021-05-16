using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers.Observables;
using System.Text.Json;

namespace RestfulFirebase.Database.Streaming
{
    internal class NodeStreamer : IDisposable
    {
        private readonly IObserver<StreamObject> observer;
        private readonly CancellationTokenSource cancel;
        private readonly IFirebaseQuery query;
        private readonly HttpClient http;

        private EventHandler<ContinueExceptionEventArgs> exceptionThrown;

        public RestfulFirebaseApp App { get; }

        internal NodeStreamer(
            RestfulFirebaseApp app,
            IObserver<StreamObject> observer,
            IFirebaseQuery query,
            EventHandler<ContinueExceptionEventArgs> exceptionThrown)
        {
            App = app;
            this.observer = observer;
            this.query = query;
            this.exceptionThrown = exceptionThrown;

            cancel = new CancellationTokenSource();

            var httpClient = App.Config.HttpStreamFactory.GetHttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            http = httpClient;
        }

        public void Dispose()
        {
            cancel.Cancel();
        }

        internal IDisposable Run()
        {
            Task.Run(ReceiveThread);
            return this;
        }

        private async void ReceiveThread()
        {
            while (true)
            {
                var url = string.Empty;
                var line = string.Empty;
                var statusCode = HttpStatusCode.OK;

                try
                {
                    cancel.Token.ThrowIfCancellationRequested();

                    url = await query.BuildUrlAsync().ConfigureAwait(false);

                    var request = App.Config.HttpStreamFactory.GetStreamHttpRequestMessage(HttpMethod.Get, url);

                    var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel.Token).ConfigureAwait(false);

                    var serverEvent = ServerEventType.KeepAlive;

                    statusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new NonBlockingStreamReader(stream))
                    {
                        while (true)
                        {
                            cancel.Token.ThrowIfCancellationRequested();
                            
                            line = (await reader.ReadLineAsync())?.Trim();

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
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    var fireEx = new FirebaseException(ExceptionHelpers.GetFailureReason(statusCode), ex);
                    var args = new ContinueExceptionEventArgs(fireEx, true);
                    exceptionThrown?.Invoke(this, args);

                    Console.WriteLine("STREAM ERROR: " + ex.Message);

                    if (!args.IgnoreAndContinue)
                    {
                        this.observer.OnError(fireEx);
                        Dispose();
                        break;
                    }
                }
                await Task.Delay(2000).ConfigureAwait(false);
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
                    var result = JsonDocument.Parse(serverData, Utils.JsonDocumentOptions);
                    var pathToken = result.RootElement.GetProperty("path");
                    var dataToken = result.RootElement.GetProperty("data");
                    var path = pathToken.ValueKind == JsonValueKind.Null ? null : pathToken.ToString();
                    List<string> separatedPath = new List<string>()
                    {
                        query.GetAbsolutePath().Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).LastOrDefault()
                    };
                    if (path != "/") separatedPath.AddRange(path.Split('/').Skip(1));

                    this.observer.OnNext(new StreamObject(Convert(dataToken), separatedPath.ToArray()));

                    break;
                case ServerEventType.KeepAlive:
                    break;
                case ServerEventType.Cancel:
                    this.observer.OnError(new FirebaseException(FirebaseExceptionReason.DatabaseUnauthorized, new Exception("Cancelled")));
                    Dispose();
                    break;
            }
        }

        private StreamData Convert(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var datas = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText(), Utils.JsonSerializerOptions);
                var subDatas = new Dictionary<string, StreamData>();
                foreach (var obj in element.EnumerateObject())
                {
                    subDatas.Add(obj.Name, Convert(obj.Value));
                }
                return new MultiStreamData(subDatas);
            }
            else if (element.ValueKind != JsonValueKind.Null)
            {
                return new SingleStreamData(element.ToString());
            }
            else if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            else
            {
                throw new Exception("Unknown stream data type");
            }
        }
    }
}
