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
using RestfulFirebase.Common.Requests;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections;
using System.IO;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using RestfulFirebase.CloudFirestore.Query;
using System.Data;

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

    internal const string RequiresUnreferencedCodeMessage = $"Model objects must preserve all its required types when trimming is enabled";
    internal const string FirestoreDatabaseDocumentsEndpoint = "https://firestore.googleapis.com/v1/projects/{0}/databases/{1}/documents/{2}";

    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();
    private static readonly object?[] parseDictionaryFieldsReflectionKey = new object?[1];
    private static readonly object?[] parseCollectionFieldsAddMethodParameter = new object?[1];

    #endregion

    #region Helpers

    internal static async Task<string> ExecuteWithGet(FirestoreDatabaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        string responseData = "N/A";
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

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
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

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

    internal static async Task<string> ExecuteWithPatchContent(FirestoreDatabaseRequest request, string postContent)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        string responseData = "N/A";
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            HttpRequestMessage msg = new(new HttpMethod("PATCH"), uri)
            {
                Content = new StringContent(postContent, Encoding.UTF8, "Application/json")
            };

            var response = await httpClient.SendAsync(msg, request.CancellationToken);

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
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    internal static Document<T>? ParseDocument<T>(T? existingModel, JsonElement.ObjectEnumerator jsonElementEnumerator, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? CamelCaseJsonSerializerOption.PropertyNamingPolicy;

        bool isSameProperty(PropertyInfo propertyInfo, string name)
        {
            if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
            {
                return false;
            }

            var attr = propertyInfo.GetCustomAttribute(typeof(JsonPropertyNameAttribute));

            if (attr is JsonPropertyNameAttribute jsonPropertyNameAttribute)
            {
                return jsonPropertyNameAttribute.Name.Equals(name);
            }
            else
            {
                return (jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name).Equals(name);
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        object? parseJsonElement(string documentFieldType, JsonElement documentFieldValue, Type objType, object? obj)
        {
            switch (documentFieldType)
            {
                case "nullValue":
                    break;
                case "booleanValue":
                case "integerValue":
                case "doubleValue":
                case "timestampValue":
                case "stringValue":
                case "bytesValue":
                    try
                    {
                        obj = JsonSerializer.Deserialize(
                            documentFieldValue.GetRawText(),
                            objType,
                            jsonSerializerOptions);
                    }
                    catch { }
                    break;
                case "referenceValue":
                    if (objType == typeof(DocumentReference))
                    {
                        string? reference = JsonSerializer.Deserialize<string>(documentFieldValue.GetRawText(), jsonSerializerOptions);
                        if (reference != null && !string.IsNullOrEmpty(reference))
                        {
                            string[] paths = reference.Split('/');
                            object currentPath = Database(paths[3]);

                            for (int i = 5; i < paths.Length; i++)
                            {
                                if (currentPath is Database database)
                                {
                                    currentPath = database.Collection(paths[i]);
                                }
                                else if (currentPath is CollectionReference colPath)
                                {
                                    currentPath = colPath.Document(paths[i]);
                                }
                                else if (currentPath is DocumentReference docPath)
                                {
                                    currentPath = docPath.Collection(paths[i]);
                                }
                            }

                            if (currentPath is DocumentReference documentReference)
                            {
                                obj = documentReference;
                            }
                        }
                    }
                    break;
                case "geoPointValue":
                    if (objType.GetInterfaces().Any(i => i == typeof(IGeoPoint)))
                    {
                        double? latitude = default;
                        double? longitude = default;
                        foreach (var geoProperty in documentFieldValue.EnumerateObject())
                        {
                            if (geoProperty.Name == "latitude")
                            {
                                latitude = JsonSerializer.Deserialize<double>(geoProperty.Value.GetRawText());
                            }
                            else if (geoProperty.Name == "longitude")
                            {
                                longitude = JsonSerializer.Deserialize<double>(geoProperty.Value.GetRawText());
                            }
                        }

                        obj ??= Activator.CreateInstance(objType);

                        if (latitude.HasValue && longitude.HasValue)
                        {
                            objType.GetProperty(nameof(IGeoPoint.Latitude))?.SetValue(obj, latitude.Value);
                            objType.GetProperty(nameof(IGeoPoint.Longitude))?.SetValue(obj, longitude.Value);
                        }
                    }
                    break;
                case "arrayValue":
                    var arrayEnumerator = documentFieldValue.EnumerateObject().FirstOrDefault().Value.EnumerateArray();

                    if (objType.IsArray && objType.GetArrayRank() == 1)
                    {
                        Type? arrayElementType = objType.GetElementType();

                        if (arrayElementType != null)
                        {
                            obj = parseArrayFields(arrayElementType, arrayEnumerator);
                        }
                    }
                    else
                    {
                        var collectionInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ICollection<>));

                        if (collectionInterfaceType != null)
                        {
                            obj ??= Activator.CreateInstance(objType);

                            if (obj != null)
                            {
                                Type[] dictionaryGenericArgsType = collectionInterfaceType.GetGenericArguments();

                                parseCollectionFields(collectionInterfaceType, dictionaryGenericArgsType[0], obj, arrayEnumerator);
                            }
                        }
                    }

                    break;
                case "mapValue":
                    var mapEnumerator = documentFieldValue.EnumerateObject().FirstOrDefault().Value.EnumerateObject();

                    var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                    obj ??= Activator.CreateInstance(objType);

                    if (obj != null)
                    {
                        if (dictionaryInterfaceType != null)
                        {
                            Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();

                            parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, mapEnumerator);
                        }
                        else
                        {
                            parseObjectFields(objType, obj, mapEnumerator);
                        }
                    }
                    break;
            }

            return obj;
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectField(Type objType, object modelObject, JsonProperty fieldProperty)
        {
            string? fieldName = fieldProperty.Name;

            var fieldInfo = objType.GetProperties().FirstOrDefault(i => isSameProperty(i, fieldName));

            if (fieldInfo == null)
            {
                return;
            }

            var field = fieldProperty.Value.EnumerateObject().FirstOrDefault();
            var fieldType = field.Name;
            var subObjType = fieldInfo.PropertyType;

            object? subObj = fieldInfo.GetValue(modelObject);

            object? parsedSubObj = parseJsonElement(fieldType, field.Value, subObjType, subObj);

            fieldInfo.SetValue(modelObject, parsedSubObj);
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        object? parseArrayFields(Type valueType, JsonElement.ArrayEnumerator enumerator)
        {
            List<object?> items = new();

            foreach (var fieldElement in enumerator)
            {
                var documentField = fieldElement.EnumerateObject().FirstOrDefault();
                var documentFieldType = documentField.Name;
                object? parsedSubObj = parseJsonElement(documentFieldType, documentField.Value, valueType, null);

                items.Add(parsedSubObj);
            }

            Array obj = Array.CreateInstance(valueType, items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                obj.SetValue(items[i], i);
            }

            return obj;
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseCollectionFields(Type collectionInterfaceType, Type valueType, object collectionObj, JsonElement.ArrayEnumerator enumerator)
        {
            var addMethod = collectionInterfaceType.GetMethod("Add");
            var clearMethod = collectionInterfaceType.GetMethod("Clear");

            if (addMethod != null && clearMethod != null)
            {
                clearMethod.Invoke(collectionObj, emptyParameterPlaceholder);

                foreach (var fieldElement in enumerator)
                {
                    var documentField = fieldElement.EnumerateObject().FirstOrDefault();
                    var documentFieldType = documentField.Name;
                    object? parsedSubObj = parseJsonElement(documentFieldType, documentField.Value, valueType, null);

                    parseCollectionFieldsAddMethodParameter[0] = parsedSubObj;

                    addMethod.Invoke(collectionObj, parseCollectionFieldsAddMethodParameter);
                }
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseDictionaryFields(Type dictionaryInterfaceType, Type keyType, Type valueType, object dictionaryObj, JsonElement.ObjectEnumerator enumerator)
        {
            var itemProperty = dictionaryInterfaceType.GetProperty("Item");
            var containsKeyMethod = dictionaryInterfaceType.GetMethod("ContainsKey");

            if (itemProperty != null && containsKeyMethod != null)
            {
                foreach (var fieldProperty in enumerator)
                {
                    var documentField = fieldProperty.Value.EnumerateObject().FirstOrDefault();
                    var documentFieldType = documentField.Name;
                    string? documentFieldKey = $"\"{fieldProperty.Name}\"";

                    var objKey = JsonSerializer.Deserialize(
                        documentFieldKey,
                        keyType,
                        jsonSerializerOptions);
                    parseDictionaryFieldsReflectionKey[0] = objKey;

                    object? subObj = default;
                    if (containsKeyMethod.Invoke(dictionaryObj, parseDictionaryFieldsReflectionKey) is bool containsKey && containsKey)
                    {
                        subObj = itemProperty.GetValue(dictionaryObj, parseDictionaryFieldsReflectionKey);
                    }

                    object? parsedSubObj = parseJsonElement(documentFieldType, documentField.Value, valueType, subObj);

                    itemProperty.SetValue(dictionaryObj, parsedSubObj, parseDictionaryFieldsReflectionKey);
                }
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, JsonElement.ObjectEnumerator enumerator)
        {
            foreach (var fieldProperty in enumerator)
            {
                parseObjectField(objType, obj, fieldProperty);
            }
        }

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
                    parseObjectFields(typeof(T), model, documentProperty.Value.EnumerateObject());
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    internal static string? PopulateDocument<T>(T model, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonElement modelElement = JsonSerializer.SerializeToElement(model, jsonSerializerOptions);

        List<(string name, string type, object? value)> fields = new();

        foreach (var property in modelElement.EnumerateObject())
        {
            string name = property.Name;
            string? type = null;
            object? value = null;
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Null:
                    type = "nullValue";
                    break;
                case JsonValueKind.True:
                    type = "booleanValue";
                    value = true;
                    break;
                case JsonValueKind.False:
                    type = "booleanValue";
                    value = false;
                    break;
                case JsonValueKind.Number:
                    var val = property.Value.ToString();
                    value = val;
                    if (val.Contains('.'))
                    {
                        type = "doubleValue";
                    }
                    else
                    {
                        type = "integerValue";
                    }
                    break;
                case JsonValueKind.String:
                    type = "stringValue";
                    value = property.Value.ToString();
                    break;
                case JsonValueKind.Array:
                    //type = "arrayValue";
                    break;
                default:
                    if (property.Value.TryGetDateTime(out DateTime dateTime))
                    {
                        type = "timestampValue";
                        value = dateTime;
                    }
                    else if (property.Value.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffset))
                    {
                        type = "timestampValue";
                        value = dateTimeOffset;
                    }
                    else if (property.Value.TryGetBytesFromBase64(out byte[]? base64))
                    {
                        type = "bytesValue";
                        value = base64;
                    }
                    //else if (property.Value.TryGetBytesFromBase64(out byte[]? base64))
                    //{
                    //    type = "bytesValue";
                    //}
                    //type = "referenceValue";
                    //type = "geoPointValue";
                    //type = "mapValue";
                    break;
            }
            if (type != null)
            {
                fields.Add((name, type, value));
            }
        }

        StringBuilder sb = new();

        sb.Append("{\"fields\":{");

        for (int i = 0; i < fields.Count; i++)
        {
            string name = fields[i].name;
            string type = fields[i].type;
            object? value = fields[i].value;

            sb.Append($"\"{name}\":{{\"{type}\":\"{value}\"}}");

            if (i < fields.Count - 1)
            {
                sb.Append(',');
            }
        }

        sb.Append("}}");

        string? content = sb.ToString();

        return content;
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

        return exception ?? new FirestoreDatabaseUndefinedException(originalException, responseData, statusCode);
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
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="GetDocumentRequest{T}.Reference"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<Document<T>?> GetDocument<T>(GetDocumentRequest<T> request, JsonSerializerOptions? jsonSerializerOptions = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Reference);

        jsonSerializerOptions ??= CamelCaseJsonSerializerOption;

        var responseData = await ExecuteWithGet(request);

        return ParseDocument(request.Model, JsonDocument.Parse(responseData).RootElement.EnumerateObject(), jsonSerializerOptions);
    }

    /// <summary>
    /// Patch the <see cref="Document{T}"/> of the specified request query.
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
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/>,
    /// <see cref="GetDocumentRequest{T}.Model"/> and
    /// <see cref="GetDocumentRequest{T}.Reference"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    public static async Task<Document<T>?> PatchDocument<T>(PatchDocumentRequest<T> request, JsonSerializerOptions? jsonSerializerOptions = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(request.Model);
        ArgumentNullException.ThrowIfNull(request.Reference);

        jsonSerializerOptions ??= CamelCaseJsonSerializerOption;

        string? content = PopulateDocument(request.Model, jsonSerializerOptions);

        if (content == null)
        {
            throw new Exception();
        }

        var responseData = await ExecuteWithPatchContent(request, content);

        return ParseDocument(request.Model, JsonDocument.Parse(responseData).RootElement.EnumerateObject(), jsonSerializerOptions);
    }

    #endregion
}
