using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries2;

public partial class QueryRoot
{
    internal HttpClient GetClient()
    {
        Client ??= App.GetHttpClient();

        return Client;
    }

    internal async ValueTask<HttpResponse<string>> BuildUrl(CancellationToken cancellationToken = default)
    {
        HttpResponse<string> response = new();

        var segementResponse = await BuildUrlSegement(cancellationToken);
        response.Append(segementResponse);
        if (segementResponse.IsError)
        {
            return response;
        }

        string segement = "";
        if (!string.IsNullOrEmpty(segementResponse.Result))
        {
            segement = $"?{segementResponse.Result}";
        }

        response.Append($"{Reference.Url}.json{segement}");

        return response;
    }

    internal virtual async ValueTask<HttpResponse<string>> BuildUrlSegement(CancellationToken cancellationToken = default)
    {
        if (LastQuery == null)
        {
            return await SegementFactory(cancellationToken);
        }
        else
        {
            HttpResponse<string> response = new();

            var lastSegementResponse = await LastQuery.BuildUrlSegement(cancellationToken);
            response.Append(lastSegementResponse);
            if (lastSegementResponse.IsError)
            {
                return response;
            }

            var segementResponse = await SegementFactory(cancellationToken);
            response.Append(segementResponse);
            if (segementResponse.IsError)
            {
                return response;
            }

            if (string.IsNullOrEmpty(lastSegementResponse.Result))
            {
                response.Append($"{segementResponse.Result}");
            }
            else
            {
                response.Append($"{lastSegementResponse.Result}&{segementResponse.Result}");
            }

            return response;
        }
    }

    public ValueTask<HttpResponse<string>> Build()
    {
        return BuildUrl();
    }
}
