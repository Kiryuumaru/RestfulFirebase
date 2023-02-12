using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.Storage.Enums;
using RestfulFirebase.Storage.Exceptions;
using RestfulHelpers.Interface;
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

namespace RestfulFirebase.Storage;

public partial class StorageApi
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ErrorData))]
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

        StorageErrorType errorType = lastTransaction?.StatusCode switch
        {
            //400
            HttpStatusCode.BadRequest => StorageErrorType.BadRequestException,
            //401
            HttpStatusCode.Unauthorized => StorageErrorType.UnauthorizedException,
            //402
            HttpStatusCode.PaymentRequired => StorageErrorType.PaymentRequiredException,
            //403
            HttpStatusCode.Forbidden => StorageErrorType.UnauthorizedException,
            //404
            HttpStatusCode.NotFound => StorageErrorType.NotFoundException,
            //412
            HttpStatusCode.PreconditionFailed => StorageErrorType.PreconditionFailedException,
            //500
            HttpStatusCode.InternalServerError => StorageErrorType.InternalServerErrorException,
            //503
            HttpStatusCode.ServiceUnavailable => StorageErrorType.ServiceUnavailableException,
            //Unknown
            _ => StorageErrorType.UndefinedException,
        };

        return new StorageException(errorType, message ?? "Unknown error occured.", requestUrlStr, requestContentStr, responseContentStr, lastTransaction?.StatusCode, response.Error);
    }
}
