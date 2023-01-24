using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulHelpers;
using RestfulHelpers.Common;
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

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    internal const string FirestoreDatabaseV1Endpoint = "https://firestore.googleapis.com/v1";
    internal const string FirestoreDatabaseDocumentsEndpoint = "projects/{0}/databases/(default)/documents{1}";

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
    internal async Task<HttpResponse<T>> ExecuteGet<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IAuthorization? authorization, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var clientResponse = await GetHttpClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError || clientResponse.Result == null)
        {
            return response;
        }

        var getResponse = await clientResponse.Result.Execute<T>(HttpMethod.Get, url, jsonSerializerOptions, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePost(IAuthorization? authorization, MemoryStream stream, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var clientResponse = await GetHttpClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError || clientResponse.Result == null)
        {
            return response;
        }

        var postResponse = await clientResponse.Result.ExecuteWithContent(stream, HttpMethod.Post, url, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<T>> ExecutePost<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IAuthorization? authorization, MemoryStream stream, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var clientResponse = await GetHttpClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError || clientResponse.Result == null)
        {
            return new HttpResponse<T>(default, clientResponse);
        }

        var postResponse = await clientResponse.Result.ExecuteWithContent<T>(stream, HttpMethod.Post, url, jsonSerializerOptions, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            response.Append(await GetHttpException(response));

            return response;
        }

        return response;
    }

    internal static void BuildTransactionOption<TTransaction>(Utf8JsonWriter writer, TTransaction transaction)
        where TTransaction : Transaction
    {
        if (transaction is ReadOnlyTransaction readOnlyTransaction)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("readOnly");
            writer.WriteStartObject();
            if (readOnlyTransaction.ReadTime.HasValue)
            {
                writer.WritePropertyName("readTime");
                writer.WriteStringValue(readOnlyTransaction.ReadTime.Value.ToUniversalTime());
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        else if (transaction is ReadWriteTransaction readWriteTransaction)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("readWrite");
            writer.WriteStartObject();
            if (readWriteTransaction.RetryTransaction != null)
            {
                writer.WritePropertyName("retryTransaction");
                writer.WriteStringValue(readWriteTransaction.RetryTransaction);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }

    internal static void BuildTransaction<TTransaction>(Utf8JsonWriter writer, TTransaction transaction)
        where TTransaction : Transaction
    {
        if (transaction.Token == null)
        {
            ArgumentException.Throw($"\"{nameof(Transaction)}\" is provided but missing token. \"{nameof(Transaction)}\" must be created first by passing the parameter to any read operations.");
        }
        writer.WritePropertyName("transaction");
        writer.WriteStringValue(transaction.Token);
    }
}
