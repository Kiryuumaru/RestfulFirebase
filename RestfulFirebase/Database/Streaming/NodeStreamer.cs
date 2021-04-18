using Newtonsoft.Json.Linq;
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
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;

namespace RestfulFirebase.Database.Streaming
{
    internal class NodeStreamer : IDisposable
    {
        private readonly IObserver<StreamObject> observer;
        private readonly CancellationTokenSource cancel;
        private readonly IFirebaseQuery query;

        private readonly HttpClient http;

        private EventHandler<ContinueExceptionEventArgs> exceptionThrown;

        internal NodeStreamer(IObserver<StreamObject> observer, IFirebaseQuery query, EventHandler<ContinueExceptionEventArgs> exceptionThrown)
        {
            this.observer = observer;
            this.query = query;
            this.exceptionThrown = exceptionThrown;
            cancel = new CancellationTokenSource();

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
                CookieContainer = new CookieContainer()
            };

            var httpClient = new HttpClient(handler, true);

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

                    // initialize network connection
                    url = await query.BuildUrlAsync().ConfigureAwait(false);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var serverEvent = ServerEventType.KeepAlive;

                    var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel.Token).ConfigureAwait(false);

                    statusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new NonBlockingStreamReader(stream))
                    {
                        while (true)
                        {
                            cancel.Token.ThrowIfCancellationRequested();

                            line = reader.ReadLine()?.Trim();

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
                    var fireEx = new FirebaseException(url, string.Empty, line, statusCode, ex);
                    var args = new ContinueExceptionEventArgs(fireEx, statusCode == HttpStatusCode.OK);
                    exceptionThrown?.Invoke(this, args);

                    if (!args.IgnoreAndContinue)
                    {
                        this.observer.OnError(new FirebaseException(url, string.Empty, line, statusCode, ex));
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
                    var result = JObject.Parse(serverData);
                    var pathToken = result["path"];
                    var dataToken = result["data"];
                    var path = pathToken.Type == JTokenType.Null ? null : pathToken.ToString();
                    var data = dataToken.Type == JTokenType.Null ? null : dataToken.ToString();
                    List<string> separatedPath = new List<string>()
                    {
                        query.GetAbsolutePath().Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).LastOrDefault()
                    };
                    if (path != "/") separatedPath.AddRange(path.Split('/').Skip(1));

                    this.observer.OnNext(new StreamObject(data, separatedPath.ToArray()));

                    break;
                case ServerEventType.KeepAlive:
                    break;
                case ServerEventType.Cancel:
                    this.observer.OnError(new FirebaseException(url, string.Empty, serverData, HttpStatusCode.Unauthorized));
                    Dispose();
                    break;
            }
        }
    }
}
