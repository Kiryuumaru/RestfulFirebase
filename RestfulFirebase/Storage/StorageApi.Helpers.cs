using RestfulFirebase.Common.Utilities;
using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using RestfulHelpers;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RestfulFirebase.Storage;

public partial class StorageApi
{
    internal const string FirebaseStorageEndpoint = "https://firebasestorage.googleapis.com/v0/b/";

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

    internal async Task<HttpResponse<HttpClient>> GetHttpClient(IAuthorization? authorization, CancellationToken cancellationToken)
    {
        HttpResponse<HttpClient> response = new();

        var client = App.GetHttpClient();
        response.Append(client);

        if (authorization == null)
        {
            return response;
        }

        var tokenResponse = await authorization.GetFreshToken(cancellationToken);
        response.Append(tokenResponse);
        if (tokenResponse.IsError)
        {
            return response;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Result);

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<T>> ExecuteGet<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string url, IAuthorization? authorization, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var getHttpClientResponse = await App.Storage.GetHttpClient(authorization, cancellationToken);
        response.Append(getHttpClientResponse);
        if (getHttpClientResponse.IsError)
        {
            return response;
        }

        var getResponse = await getHttpClientResponse.Result.Execute<T>(HttpMethod.Get, url, JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<T>> Execute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(HttpRequestMessage httpRequestMessage, IAuthorization? authorization, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var getHttpClientResponse = await App.Storage.GetHttpClient(authorization, cancellationToken);
        response.Append(getHttpClientResponse);
        if (getHttpClientResponse.IsError)
        {
            return response;
        }

        var getResponse = await getHttpClientResponse.Result.Execute<T>(httpRequestMessage, JsonSerializerHelpers.CamelCaseJsonSerializerOption, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }

    internal async Task<HttpResponse> ExecuteDelete(IAuthorization? authorization, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var getHttpClientResponse = await App.Storage.GetHttpClient(authorization, cancellationToken);
        response.Append(getHttpClientResponse);
        if (getHttpClientResponse.IsError)
        {
            return response;
        }

        var getResponse = await getHttpClientResponse.Result.Execute(HttpMethod.Delete, url, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }
}