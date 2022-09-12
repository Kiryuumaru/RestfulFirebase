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
using static System.Text.Json.JsonElement;
using System.Xml.Linq;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static class FirestoreDatabase
{
    #region Properties

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    internal const string RequiresUnreferencedCodeMessage = $"Model objects must preserve all its required types when trimming is enabled";
    internal const string FirestoreDatabaseV1Endpoint = "https://firestore.googleapis.com/v1/";
    internal const string FirestoreDatabaseDocumentsEndpoint = "projects/{0}/databases/{1}/documents/{2}";

    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();

    #endregion

    #region Helpers

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
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }
    }

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

    internal static async Task<string> ExecuteWithDelete(FirestoreDatabaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        string responseData = "N/A";
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            var response = await httpClient.DeleteAsync(
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
    internal static Document<T>? ParseDocument<T>(T? existingObj, ObjectEnumerator jsonElementEnumerator, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

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
        object? parseJsonElement(JsonElement jsonElement, Type objType, object? obj)
        {
            JsonConverter? jsonConverter = jsonSerializerOptions.Converters.FirstOrDefault(i => i.CanConvert(objType));

            if (jsonConverter != null)
            {
                try
                {
                    obj = JsonSerializer.Deserialize(
                        jsonElement,
                        objType,
                        jsonSerializerOptions);
                }
                catch { }
            }
            else
            {
                var documentField = jsonElement.EnumerateObject().FirstOrDefault();
                string documentFieldType = documentField.Name;
                JsonElement documentFieldValue = documentField.Value;
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
                                    Type[] collectionGenericArgsType = collectionInterfaceType.GetGenericArguments();

                                    parseCollectionFields(collectionInterfaceType, collectionGenericArgsType[0], obj, arrayEnumerator);
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
            }

            return obj;
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        object? parseArrayFields(Type valueType, ArrayEnumerator enumerator)
        {
            List<object?> items = new();

            foreach (var fieldElement in enumerator)
            {
                object? parsedSubObj = parseJsonElement(fieldElement, valueType, null);

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
        void parseCollectionFields(Type collectionInterfaceType, Type valueType, object collectionObj, ArrayEnumerator enumerator)
        {
            var addMethod = collectionInterfaceType.GetMethod("Add");
            var clearMethod = collectionInterfaceType.GetMethod("Clear");
            var addMethodParameter = new object?[1];

            if (addMethod != null && clearMethod != null)
            {
                clearMethod.Invoke(collectionObj, emptyParameterPlaceholder);

                foreach (var fieldElement in enumerator)
                {
                    object? parsedSubObj = parseJsonElement(fieldElement, valueType, null);

                    addMethodParameter[0] = parsedSubObj;

                    addMethod.Invoke(collectionObj, addMethodParameter);
                }
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseDictionaryFields(Type dictionaryInterfaceType, Type keyType, Type valueType, object dictionaryObj, ObjectEnumerator enumerator)
        {
            var itemProperty = dictionaryInterfaceType.GetProperty("Item");
            var containsKeyMethod = dictionaryInterfaceType.GetMethod("ContainsKey");
            var keyParameter = new object?[1];

            if (itemProperty != null && containsKeyMethod != null)
            {
                foreach (var fieldProperty in enumerator)
                {
                    string? documentFieldKey = $"\"{fieldProperty.Name}\"";

                    object? objKey = JsonSerializer.Deserialize(
                            documentFieldKey,
                            keyType,
                            jsonSerializerOptions);

                    keyParameter[0] = objKey;

                    object? subObj = default;
                    if (containsKeyMethod.Invoke(dictionaryObj, keyParameter) is bool containsKey && containsKey)
                    {
                        subObj = itemProperty.GetValue(dictionaryObj, keyParameter);
                    }

                    object? parsedSubObj = parseJsonElement(fieldProperty.Value, valueType, subObj);

                    itemProperty.SetValue(dictionaryObj, parsedSubObj, keyParameter);
                }
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, ObjectEnumerator enumerator)
        {
            foreach (var fieldProperty in enumerator)
            {
                string? fieldName = fieldProperty.Name;

                var fieldInfo = objType.GetProperties().FirstOrDefault(i => isSameProperty(i, fieldName));

                if (fieldInfo == null)
                {
                    return;
                }

                var subObjType = fieldInfo.PropertyType;

                object? subObj = fieldInfo.GetValue(obj);

                object? parsedSubObj = parseJsonElement(fieldProperty.Value, subObjType, subObj);

                fieldInfo.SetValue(obj, parsedSubObj);
            }
        }

        string? name = default;
        DateTimeOffset? createTime = default;
        DateTimeOffset? updateTime = default;
        T obj = existingObj ?? Activator.CreateInstance<T>();
        Type objType = typeof(T);

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
                    var mapEnumerator = documentProperty.Value.EnumerateObject();

                    var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                    if (dictionaryInterfaceType != null)
                    {
                        Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();

                        parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, mapEnumerator);
                    }
                    else
                    {
                        parseObjectFields(objType, obj, mapEnumerator);
                    }

                    break;
            }
        }

        Document<T>? document;

        if (name != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            document = new Document<T>(name, obj, createTime.Value, updateTime.Value);
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
    internal static string? PopulateDocument<T>(FirebaseConfig config, T obj, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        StringBuilder sb = new();

        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObject(Type objType, object? obj, string headJson, string tailJson, Action onFirstAppend)
        {
            bool hasAppended = false;

            JsonConverter? jsonConverter = jsonSerializerOptions.Converters.FirstOrDefault(i => i.CanConvert(objType));

            if (jsonConverter != null)
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized) && serialized != "null")
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append(serialized);
                }
                else
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"nullValue\":null}}");
                }
            }
            else if (obj == null)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend();
                    sb.Append(headJson);
                }
                sb.Append($"{{\"nullValue\":null}}");
            }
            else if (objType.IsAssignableFrom(typeof(bool)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"booleanValue\":\"{serialized}\"}}");
                }
            }
            else if (
                objType.IsAssignableFrom(typeof(sbyte)) ||
                objType.IsAssignableFrom(typeof(byte)) ||
                objType.IsAssignableFrom(typeof(short)) ||
                objType.IsAssignableFrom(typeof(ushort)) ||
                objType.IsAssignableFrom(typeof(int)) ||
                objType.IsAssignableFrom(typeof(uint)) ||
                objType.IsAssignableFrom(typeof(long)) ||
                objType.IsAssignableFrom(typeof(ulong)) ||
                objType.IsAssignableFrom(typeof(nint)) ||
                objType.IsAssignableFrom(typeof(nuint)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"integerValue\":\"{serialized}\"}}");
                }
            }
            else if (
                objType.IsAssignableFrom(typeof(float)) ||
                objType.IsAssignableFrom(typeof(double)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"doubleValue\":\"{serialized}\"}}");
                }
            }
            else if (objType.IsAssignableFrom(typeof(decimal)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"stringValue\":\"{serialized}\"}}");
                }
            }
            else if (
                objType.IsAssignableFrom(typeof(DateTime)) ||
                objType.IsAssignableFrom(typeof(DateTimeOffset)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null)
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"timestampValue\":{serialized}}}");
                }
            }
            else if (objType.IsAssignableFrom(typeof(string)))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"stringValue\":{serialized}}}");
                }
            }
            else if (objType.IsAssignableFrom(typeof(byte[])))
            {
                string? serialized = null;
                try
                {
                    serialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
                }
                catch { }
                if (serialized != null && !string.IsNullOrWhiteSpace(serialized))
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"bytesValue\":{serialized}}}");
                }
            }
            else if (obj is DocumentReference documentReference)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend();
                    sb.Append(headJson);
                }
                sb.Append($"{{\"referenceValue\":\"{documentReference.BuildUrlCascade(config.ProjectId)}\"}}");
            }
            else if (obj is IGeoPoint geoPoint)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend();
                    sb.Append(headJson);
                }
                sb.Append($"{{\"geoPointValue\":{{\"latitude\":{geoPoint.Latitude},\"longitude\":{geoPoint.Longitude}}}}}");
            }
            else if (objType.IsArray && objType.GetArrayRank() == 1 && objType.GetElementType() is Type elementType && obj is Array array)
            {
                if (array.Length == 0)
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                    sb.Append($"{{\"arrayValue\":{{\"values\":[]}}}}");
                }
                else
                {
                    parseArrayFields(elementType, array, $"{{\"arrayValue\":{{\"values\":[", $"]}}}}", () =>
                    {
                        if (!hasAppended)
                        {
                            hasAppended = true;
                            onFirstAppend();
                            sb.Append(headJson);
                        }
                    });
                }
            }
            else
            {
                while (true)
                {
                    var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                    var collectionInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ICollection<>));

                    if (dictionaryInterfaceType != null && collectionInterfaceType != null)
                    {
                        Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();
                        Type[] dictionaryGenericCollectionArgsType = collectionInterfaceType.GetGenericArguments();

                        parseDictionaryFields(
                            dictionaryGenericCollectionArgsType[0],
                            dictionaryGenericArgsType[0],
                            dictionaryGenericArgsType[1],
                            (IEnumerable)obj,
                            $"{{\"mapValue\":{{\"fields\":{{",
                            $"}}}}}}",
                            () =>
                            {
                                if (!hasAppended)
                                {
                                    hasAppended = true;
                                    onFirstAppend();
                                    sb.Append(headJson);
                                }
                            });

                        break;
                    }

                    if (collectionInterfaceType != null)
                    {
                        Type[] collectionGenericArgsType = collectionInterfaceType.GetGenericArguments();

                        parseCollectionFields(
                            collectionGenericArgsType[0],
                            (IEnumerable)obj,
                            $"{{\"arrayValue\":{{\"values\":[",
                            $"]}}}}",
                            () =>
                            {
                                if (!hasAppended)
                                {
                                    hasAppended = true;
                                    onFirstAppend();
                                    sb.Append(headJson);
                                }
                            });

                        break;
                    }

                    parseObjectFields(objType, obj, "{\"mapValue\":{\"fields\":{", "}}}", () =>
                    {
                        if (!hasAppended)
                        {
                            hasAppended = true;
                            onFirstAppend();
                            sb.Append(headJson);
                        }
                    });

                    break;
                }
            }

            if (hasAppended)
            {
                sb.Append(tailJson);
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseArrayFields(Type elementType, Array arrayObj, string headJson, string tailJson, Action onFirstAppend)
        {
            bool hasAppended = false;
            foreach (var obj in arrayObj)
            {
                parseObject(elementType, obj, "", ",", () =>
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                });
            }
            if (hasAppended)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append(tailJson);
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseCollectionFields(Type elementType, IEnumerable collectionObj, string headJson, string tailJson, Action onFirstAppend)
        {
            bool hasAppended = false;
            foreach (var obj in collectionObj)
            {
                parseObject(elementType, obj, "", ",", () =>
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                });
            }
            if (hasAppended)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append(tailJson);
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseDictionaryFields(Type keyValuePairType, Type keyType, Type valueType, IEnumerable dictionaryObj, string headJson, string tailJson, Action onFirstAppend)
        {
            bool hasAppended = false;
            var keyProperty = keyValuePairType.GetProperty("Key");
            var valueProperty = keyValuePairType.GetProperty("Value");

            foreach (var obj in dictionaryObj)
            {
                string? key = keyProperty?.GetValue(obj)?.ToString();
                object? value = valueProperty?.GetValue(obj);
                if (key != null)
                {
                    parseObject(valueType, value, $"\"{key}\":", ",", () =>
                    {
                        if (!hasAppended)
                        {
                            hasAppended = true;
                            onFirstAppend();
                            sb.Append(headJson);
                        }
                    });
                }
            }
            if (hasAppended)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append(tailJson);
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, string headJson, string tailJson, Action onFirstAppend)
        {
            bool hasAppended = false;
            var properties = objType.GetProperties().Where(i => i.CanRead);

            foreach (var property in properties)
            {
                Type propertyType = property.PropertyType;
                object? propertyObj = property.GetValue(obj);
                string name = jsonNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                parseObject(propertyType, propertyObj, $"\"{name}\":", ",", () =>
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend();
                        sb.Append(headJson);
                    }
                });
            }
            if (hasAppended)
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append(tailJson);
            }
        }

        Type objType = typeof(T);
        bool hasAppended = false;

        var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        var collectionInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(ICollection<>));

        if (dictionaryInterfaceType != null && collectionInterfaceType != null)
        {
            Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();
            Type[] dictionaryGenericCollectionArgsType = collectionInterfaceType.GetGenericArguments();

            parseDictionaryFields(
                dictionaryGenericCollectionArgsType[0],
                dictionaryGenericArgsType[0],
                dictionaryGenericArgsType[1],
                (IEnumerable)obj,
                "{\"fields\":{",
                "}}", () =>
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                    }
                });
        }
        else
        {
            parseObjectFields(
                objType,
                obj,
                "{\"fields\":{",
                "}}", () =>
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                    }
                });
        }

        string? content = sb.ToString();

        if (hasAppended)
        {
            return content;
        }
        else
        {
            return null;
        }
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
        return RestfulFirebase.FirestoreDatabase.Database.Get(databaseId);
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
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        jsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);
        
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
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Model);
        ArgumentNullException.ThrowIfNull(request.Reference);

        jsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        string? content = PopulateDocument(request.Config, request.Model, jsonSerializerOptions);

        string? responseData;

        if (content != null)
        {
            responseData = await ExecuteWithPatchContent(request, content);
        }
        else
        {
            responseData = await ExecuteWithGet(request);

        }

        return ParseDocument(request.Model, JsonDocument.Parse(responseData).RootElement.EnumerateObject(), jsonSerializerOptions);
    }

    /// <summary>
    /// Delete the <see cref="Document{T}"/> of the specified request query.
    /// </summary>
    /// <param name="request">
    /// The request of the operation.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="CommonRequest.Config"/> and
    /// <see cref="DeleteDocumentRequest.Reference"/> are either a null reference.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    public static async Task DeleteDocument(DeleteDocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Reference);

        await ExecuteWithDelete(request);
    }

    #endregion
}
