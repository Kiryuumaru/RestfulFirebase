using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.RealtimeDatabase.Streaming;
using RestfulFirebase.RealtimeDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class QueryRoot
{
    public async Task<HttpResponse> Run(CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = App.RealtimeDatabase.ConfigureJsonSerializerOption();

        HttpResponse response = new();

        var buildUrlResponse = await BuildUrl(cancellationToken);
        response.Append(buildUrlResponse);
        if (buildUrlResponse.IsError)
        {
            return response;
        }

        var getResponse = await App.RealtimeDatabase.ExecuteGet(buildUrlResponse.Result, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            return response;
        }

        return response;
    }

    public async Task<HttpResponse> Listen<TModel>(CancellationToken cancellationToken = default)
        where TModel : class
    {
        JsonSerializerOptions jsonSerializerOptions = App.RealtimeDatabase.ConfigureJsonSerializerOption();

        HttpResponse response = new();

        var ss = new NodeStreamer(App, this, streamObj =>
        {

        }, streamError =>
        {

        }, cancellationToken);

        ss.Run();

        return response;
    }
}
