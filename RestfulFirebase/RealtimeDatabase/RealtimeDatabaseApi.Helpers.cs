using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using RestfulHelpers;

namespace RestfulFirebase.RealtimeDatabase;

public partial class RealtimeDatabaseApi
{
    internal JsonSerializerOptions ConfigureJsonSerializerOption()
    {
        if (App.Config.JsonSerializerOptions == null)
        {
            return JsonSerializerHelpers.CamelCaseJsonSerializerOption;
        }
        else
        {
            return new JsonSerializerOptions(App.Config.JsonSerializerOptions)
            {
                IgnoreReadOnlyFields = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            };
        }
    }

    internal HttpClient GetHttpClient()
    {
        return App.GetHttpClient();
    }

    internal async Task<HttpResponse> ExecuteGet(string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var getResponse = await GetHttpClient().Execute(HttpMethod.Get, url, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<T>> ExecuteGet<T>(string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var getResponse = await GetHttpClient().Execute<T>(HttpMethod.Get, url, jsonSerializerOptions, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePut(MemoryStream stream, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var postResponse = await GetHttpClient().ExecuteWithContent(stream, HttpMethod.Put, url, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<T>> ExecutePut<T>(MemoryStream stream, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var postResponse = await GetHttpClient().ExecuteWithContent<T>(stream, HttpMethod.Put, url, jsonSerializerOptions, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePost(MemoryStream stream, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var postResponse = await GetHttpClient().ExecuteWithContent(stream, HttpMethod.Post, url, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal async Task<HttpResponse<T>> ExecutePost<T>(MemoryStream stream, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var postResponse = await GetHttpClient().ExecuteWithContent<T>(stream, HttpMethod.Post, url, jsonSerializerOptions, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePatch(MemoryStream stream, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var patchResponse = await GetHttpClient().ExecuteWithContent(stream, new HttpMethod("PATCH"), url, cancellationToken);
        response.Append(patchResponse);
        if (patchResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal async Task<HttpResponse<T>> ExecutePatch<T>(MemoryStream stream, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var patchResponse = await GetHttpClient().ExecuteWithContent<T>(stream, new HttpMethod("PATCH"), url, jsonSerializerOptions, cancellationToken);
        response.Append(patchResponse);
        if (patchResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecuteDelete(string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var deleteResponse = await GetHttpClient().Execute(HttpMethod.Delete, url, cancellationToken);
        response.Append(deleteResponse);
        if (deleteResponse.IsError)
        {
            response.Append(await GetHttpException(response));
        }

        return response;
    }
}
