using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
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

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
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
}
