using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RestfulFirebase.Database.Streaming2
{
    class Class1
    {
        private async void ReceiveThread()
        {
            while (true)
            {
                var url = string.Empty;
                var line = string.Empty;
                var statusCode = HttpStatusCode.OK;

                try
                {
                    this.cancel.Token.ThrowIfCancellationRequested();

                    // initialize network connection
                    url = await this.query.BuildUrlAsync().ConfigureAwait(false);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var serverEvent = FirebaseServerEventType.KeepAlive;

                    var client = this.GetHttpClient();
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, this.cancel.Token).ConfigureAwait(false);

                    statusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new NonBlockingStreamReader(stream))
                    {
                        while (true)
                        {
                            this.cancel.Token.ThrowIfCancellationRequested();

                            line = reader.ReadLine()?.Trim();

                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            var tuple = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                            switch (tuple[0].ToLower())
                            {
                                case "event":
                                    serverEvent = this.ParseServerEvent(serverEvent, tuple[1]);
                                    break;
                                case "data":
                                    this.ProcessServerData(url, serverEvent, tuple[1]);
                                    break;
                            }

                            if (serverEvent == FirebaseServerEventType.AuthRevoked)
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
                    var args = new FirebaseException(url, string.Empty, line, statusCode, ex);

                    if (!this.OnExceptionThrown(args, statusCode == HttpStatusCode.OK))
                    {
                        this.observer.OnError(new FirebaseException(url, string.Empty, line, statusCode, ex));
                        this.Dispose();
                        break;
                    }

                    await Task.Delay(2000).ConfigureAwait(false);
                }
            }
        }
    }
}
