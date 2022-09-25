using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Authentication.Requests;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// The base implementation for all firebase cloud firestore request.
/// </summary>
public abstract class FirestoreDatabaseRequest<TResponse> : TransactionRequest<TResponse>, IAuthenticatedRequest<IAuthorization>
    where TResponse : TransactionResponse
{
    /// <inheritdoc/>
    public IAuthorization? Authorization { get; set; }

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal override async Task<HttpClient> GetClient()
    {
        var client = HttpClient ?? new HttpClient();

        if (Authorization is Common.Models.AccessTokenAuthorization accessTokenType)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenType.Token);
        }
        else if (Authorization is FirebaseUser firebaseUser)
        {
            var tokenResponse = await Api.Authentication.GetFreshToken(new GetFreshTokenRequest()
            {
                Config = Config,
                HttpClient = HttpClient,
                CancellationToken = CancellationToken,
                Authorization = firebaseUser,
            });
            tokenResponse.ThrowIfErrorOrEmptyResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Result.IdToken);
        }

        return client;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<Exception> GetHttpException(HttpRequestMessage? request, HttpResponseMessage? response, HttpStatusCode httpStatusCode, Exception exception)
    {
        string? requestUrlStr = null;
        string? requestContentStr = null;
        string? responseStr = null;
        if (request != null)
        {
            if (request.RequestUri != null)
            {
                requestUrlStr = request.RequestUri.ToString();
            }
            if (request.Content != null)
            {
                requestContentStr = await request.Content.ReadAsStringAsync();
            }
        }
        if (response != null)
        {
            responseStr = await response.Content.ReadAsStringAsync();
        }

        string? message = null;
        try
        {
            if (responseStr != null && !string.IsNullOrEmpty(responseStr) && responseStr != "N/A")
            {
                ErrorData? errorData = JsonSerializer.Deserialize<ErrorData>(responseStr, DefaultJsonSerializerOption);
                message = errorData?.Error?.Message ?? "";
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

        FirestoreErrorType errorType = httpStatusCode switch
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

        return new FirestoreDatabaseException(errorType, message ?? "Unknown error occured.", requestUrlStr, requestContentStr, responseStr, httpStatusCode, exception);
    }

    internal static JsonSerializerOptions ConfigureJsonSerializerOption(JsonSerializerOptions? jsonSerializerOptions)
    {
        if (jsonSerializerOptions == null)
        {
            return DefaultJsonSerializerOption;
        }
        else
        {
            return new JsonSerializerOptions(jsonSerializerOptions)
            {
                IgnoreReadOnlyFields = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            };
        }
    }
}
