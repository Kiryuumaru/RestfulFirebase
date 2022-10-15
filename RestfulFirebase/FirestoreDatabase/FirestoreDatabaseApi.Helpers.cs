using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Models;
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

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal static async Task<Exception> GetHttpException(IHttpResponse response)
    {
        var lastTransaction = response.HttpTransactions.LastOrDefault();

        string? requestUrlStr = lastTransaction?.RequestMessage?.RequestUri?.ToString();
        string? requestContentStr = lastTransaction == null ? null : await lastTransaction.GetRequestContentAsString();
        string? responseContentStr = lastTransaction == null ? null : await lastTransaction.GetResponseContentAsString();

        string? message = null;
        try
        {
            if (responseContentStr != null && !string.IsNullOrEmpty(responseContentStr) && responseContentStr != "N/A")
            {
                var errorDoc = JsonDocument.Parse(responseContentStr);
                if (errorDoc != null)
                {
                    if (errorDoc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        ErrorData? errorData = errorDoc.RootElement.Deserialize<ErrorData>(JsonSerializerHelpers.CamelCaseJsonSerializerOption);
                        message = errorData?.Error?.Message ?? "";
                    }
                    else if (errorDoc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        ErrorData? errorData = errorDoc.RootElement.EnumerateArray().FirstOrDefault().Deserialize<ErrorData>(JsonSerializerHelpers.CamelCaseJsonSerializerOption);
                        message = errorData?.Error?.Message ?? "";
                    }
                }
            }
        }
        catch (JsonException)
        {
            //the response wasn't JSON - no data to be parsed
        }
        catch (Exception ex)
        {
            return ex;
        }

        FirestoreErrorType errorType = lastTransaction?.StatusCode switch
        {
            //400
            HttpStatusCode.BadRequest => FirestoreErrorType.BadRequestException,
            //401
            HttpStatusCode.Unauthorized => FirestoreErrorType.UnauthorizedException,
            //402
            HttpStatusCode.PaymentRequired => FirestoreErrorType.PaymentRequiredException,
            //403
            HttpStatusCode.Forbidden => FirestoreErrorType.UnauthorizedException,
            //404
            HttpStatusCode.NotFound => FirestoreErrorType.NotFoundException,
            //412
            HttpStatusCode.PreconditionFailed => FirestoreErrorType.PreconditionFailedException,
            //500
            HttpStatusCode.InternalServerError => FirestoreErrorType.InternalServerErrorException,
            //503
            HttpStatusCode.ServiceUnavailable => FirestoreErrorType.ServiceUnavailableException,
            //Unknown
            _ => FirestoreErrorType.UndefinedException,
        };

        return new FirestoreDatabaseException(errorType, message ?? "Unknown error occured.", requestUrlStr, requestContentStr, responseContentStr, lastTransaction?.StatusCode, response.Error);
    }

    internal async Task<HttpResponse<HttpClient>> GetClient(IAuthorization? authorization, CancellationToken cancellationToken)
    {
        HttpResponse<HttpClient> response = new();

        var client = App.GetClient();
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode("Calls RestfulFirebase.Common.Http.HttpHelpers.ExecuteWithContent<T>(HttpClient, Stream, HttpMethod, String, JsonSerializerOptions, CancellationToken)")]
#endif
    internal async Task<HttpResponse<T>> ExecuteGet<T>(IAuthorization? authorization, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var clientResponse = await GetClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError)
        {
            return response;
        }

        var getResponse = await HttpHelpers.Execute<T>(App.GetClient(), HttpMethod.Get, url, jsonSerializerOptions, cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            return response.Append(await GetHttpException(response));
        }

        return response;
    }

    internal async Task<HttpResponse> ExecutePost(IAuthorization? authorization, MemoryStream stream, string url, CancellationToken cancellationToken)
    {
        HttpResponse response = new();

        var clientResponse = await GetClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError)
        {
            return response;
        }

        var postResponse = await HttpHelpers.ExecuteWithContent(App.GetClient(), stream, HttpMethod.Post, url, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            return response.Append(await GetHttpException(response));
        }

        return response;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode("Calls RestfulFirebase.Common.Http.HttpHelpers.ExecuteWithContent<T>(HttpClient, Stream, HttpMethod, String, JsonSerializerOptions, CancellationToken)")]
#endif
    internal async Task<HttpResponse<T>> ExecutePost<T>(IAuthorization? authorization, MemoryStream stream, string url, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        HttpResponse<T> response = new();

        var clientResponse = await GetClient(authorization, cancellationToken);
        response.Append(clientResponse);
        if (clientResponse.IsError)
        {
            return new HttpResponse<T>(default, clientResponse);
        }

        var postResponse = await HttpHelpers.ExecuteWithContent<T>(App.GetClient(), stream, HttpMethod.Post, url, jsonSerializerOptions, cancellationToken);
        response.Append(postResponse);
        if (postResponse.IsError)
        {
            return response.Append(await GetHttpException(response));
        }

        return response;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
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

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal static void BuildTransaction<TTransaction>(Utf8JsonWriter writer, TTransaction transaction)
        where TTransaction : Transaction
    {
        if (transaction.Token == null)
        {
            throw new ArgumentException($"\"{nameof(Transaction)}\" is provided but missing token. \"{nameof(Transaction)}\" must be created first by passing the parameter to any read operations.");
        }
        writer.WritePropertyName("transaction");
        writer.WriteStringValue(transaction.Token);
    }
}
