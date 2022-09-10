using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using RestfulFirebase.Common.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using RestfulFirebase.Authentication.Requests;
using System.Net.Http.Headers;
using System.Threading;
using System.Linq;
using System.Reflection;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static class FirestoreDatabase
{
    #region Properties

    internal static readonly JsonSerializerOptions SnakeCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal static readonly JsonSerializerOptions CamelCaseJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal const string FirestoreDatabaseDocumentsEndpoint = "https://firestore.googleapis.com/v1/projects/{0}/databases/{1}/documents/{2}";

    #endregion

    #region Helpers

    internal static async Task<string> ExecuteWithGet(FirestoreDatabaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        string responseData = "N/A";
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request);

        try
        {
            var response = await httpClient.GetAsync(
                uri,
                request.CancellationToken);

            statusCode = response.StatusCode;

            responseData = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw GetException(responseData, statusCode, ex);
        }
    }

    internal static async Task<string> ExecuteWithPostContent(FirestoreDatabaseRequest request, string postContent)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        string responseData = "N/A";
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request);

        try
        {
            var response = await httpClient.PostAsync(
                uri,
                new StringContent(postContent, Encoding.UTF8, "Application/json"),
                request.CancellationToken);

            statusCode = response.StatusCode;

            responseData = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return responseData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw GetException(responseData, statusCode, ex);
        }
    }

    internal static async Task<HttpClient> GetClient(FirestoreDatabaseRequest request)
    {
        var client = request.HttpClient ?? new HttpClient();

        if (request.FirebaseUser != null)
        {
            string token = await Authentication.GetFreshToken(request);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode($"{nameof(T)} type must preserve when .Net 6 trimming is enabled")]
#endif
    internal static Document<T>? ParseDocument<T>(T? existingModel, JsonElement.ObjectEnumerator jsonElementEnumerator, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? CamelCaseJsonSerializerOption.PropertyNamingPolicy;
        string? name = default;
        DateTimeOffset? createTime = default;
        DateTimeOffset? updateTime = default;
        T model = existingModel ?? Activator.CreateInstance<T>();

        foreach (var documentProperty in jsonElementEnumerator)
        {
            switch (documentProperty.Name)
            {
                case "name":
                    name = documentProperty.Value.GetString();
                    break;
                case "createTime":
                    createTime = documentProperty.Value.GetDateTimeOffset();
                    break;
                case "updateTime":
                    updateTime = documentProperty.Value.GetDateTimeOffset();
                    break;
                case "fields":
                    foreach (var fieldProperty in documentProperty.Value.EnumerateObject())
                    {
                        var field = fieldProperty.Value.EnumerateObject().FirstOrDefault();
                        string? fieldName = fieldProperty.Name;

                        bool isSameProperty(PropertyInfo propertyInfo)
                        {
                            if (!propertyInfo.CanWrite)
                            {
                                return false;
                            }

                            var attr = propertyInfo.GetCustomAttribute(typeof(JsonPropertyNameAttribute));

                            if (attr is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                            {
                                return jsonPropertyNameAttribute.Name.Equals(fieldName);
                            }
                            else
                            {
                                return (jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name).Equals(fieldName);
                            }
                        }

                        var fieldInfo = typeof(T).GetProperties().FirstOrDefault(i => isSameProperty(i));

                        if (fieldInfo == null)
                        {
                            continue;
                        }

                        var propertyType = fieldInfo.PropertyType;
                        var parsedValue = JsonSerializer.Deserialize(
                            field.Value.GetRawText(),
                            propertyType,
                            jsonSerializerOptions);

                        fieldInfo.SetValue(model, parsedValue);
                    }
                    break;
            }
        }

        Document<T>? document;

        if (name != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            document = new Document<T>(name, model, createTime.Value, updateTime.Value);
        }
        else
        {
            document = null;
        }

        return document;
    }

    internal static Exception GetException(string responseData, HttpStatusCode statusCode, Exception originalException)
    {
        Exception? exception = statusCode switch
        {
            //400
            HttpStatusCode.BadRequest => new FirestoreDatabaseBadRequestException(originalException),
            //401
            HttpStatusCode.Unauthorized => new FirestoreDatabaseUnauthorizedException(originalException),
            //402
            HttpStatusCode.PaymentRequired => new FirestoreDatabasePaymentRequiredException(originalException),
            //403
            HttpStatusCode.Forbidden => new FirestoreDatabaseUnauthorizedException(originalException),
            //404
            HttpStatusCode.NotFound => new FirestoreDatabaseNotFoundException(originalException),
            //412
            HttpStatusCode.PreconditionFailed => new FirestoreDatabasePreconditionFailedException(originalException),
            //500
            HttpStatusCode.InternalServerError => new FirestoreDatabaseInternalServerErrorException(originalException),
            //503
            HttpStatusCode.ServiceUnavailable => new FirestoreDatabaseServiceUnavailableException(originalException),
            //Unknown
            _ => null,
        };

        return exception ?? new FirestoreDatabaseUndefinedException(originalException, statusCode);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an instance of <see cref="Database"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="RestfulFirebase.FirestoreDatabase.Database"/>.
    /// </returns>
    public static Database Database(string? databaseId = default)
    {
        if (databaseId == null || string.IsNullOrEmpty(databaseId))
        {
            databaseId = "(default)";
        }

        return new Database(databaseId);
    }

    /// <summary>
    /// Gets the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the model to populate the document fields.
    /// </typeparam>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to deserialize the model.
    /// </param>
    /// <returns>
    /// The created <see cref="Document{T}"/>
    /// </returns>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode($"{nameof(T)} type must preserve when .Net 6 trimming is enabled")]
#endif
    public static async Task<Document<T>?> GetDocument<T>(GetDocumentRequest<T> request, JsonSerializerOptions? jsonSerializerOptions = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Reference);

        jsonSerializerOptions ??= CamelCaseJsonSerializerOption;

        var responseData = await ExecuteWithGet(request);

        return ParseDocument(request.Model, JsonDocument.Parse(responseData).RootElement.EnumerateObject(), jsonSerializerOptions);
    }

    #endregion
}
