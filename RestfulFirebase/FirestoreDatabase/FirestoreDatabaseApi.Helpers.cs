using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using System.Reflection;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using RestfulFirebase.CloudFirestore.Query;
using System.Data;
using static System.Text.Json.JsonElement;
using RestfulFirebase.Attributes;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
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

    internal static async Task<Stream> ExecuteWithGet(FirestoreDatabaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            response = await httpClient.GetAsync(uri, request.CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            string? responseStr = response == null ? "N/A" : await response.Content.ReadAsStringAsync();
            throw GetException(responseStr, statusCode, ex);
        }
    }

    internal static async Task<Stream> ExecuteWithPostContent(FirestoreDatabaseRequest request, Stream contentStream)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            contentStream.Seek(0, SeekOrigin.Begin);

            StreamContent streamContent = new(contentStream);
            streamContent.Headers.ContentType = new("Application/json")
            {
                CharSet = Encoding.UTF8.WebName
            };

            HttpRequestMessage msg = new(HttpMethod.Post, uri)
            {
                Content = streamContent
            };

            response = await httpClient.SendAsync(msg, request.CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            string? responseStr = response == null ? "N/A" : await response.Content.ReadAsStringAsync();
            throw GetException(responseStr, statusCode, ex);
        }
    }

    internal static async Task<Stream> ExecuteWithPatchContent(FirestoreDatabaseRequest request, Stream contentStream)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            contentStream.Seek(0, SeekOrigin.Begin);

            StreamContent streamContent = new(contentStream);
            streamContent.Headers.ContentType = new("Application/json")
            {
                CharSet = Encoding.UTF8.WebName
            };

            HttpRequestMessage msg = new(new HttpMethod("PATCH"), uri)
            {
                Content = streamContent
            };

            response = await httpClient.SendAsync(msg, request.CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            string? responseStr = response == null ? "N/A" : await response.Content.ReadAsStringAsync();
            throw GetException(responseStr, statusCode, ex);
        }
    }

    internal static async Task<Stream> ExecuteWithDelete(FirestoreDatabaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Config);
        ArgumentNullException.ThrowIfNull(request.Query);

        HttpClient httpClient = await GetClient(request);

        HttpResponseMessage? response = null;
        HttpStatusCode statusCode = HttpStatusCode.OK;
        string uri = request.Query.BuildUrl(request.Config.ProjectId);

        try
        {
            response = await httpClient.DeleteAsync(uri, request.CancellationToken);

            statusCode = response.StatusCode;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            string? responseStr = response == null ? "N/A" : await response.Content.ReadAsStringAsync();
            throw GetException(responseStr, statusCode, ex);
        }
    }

    internal static async Task<HttpClient> GetClient(FirestoreDatabaseRequest request)
    {
        var client = request.HttpClient ?? new HttpClient();

        if (request.FirebaseUser != null)
        {
            var tokenRequest = await Authentication.GetFreshToken(request);
            tokenRequest.ThrowIfErrorOrEmptyResponse();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenRequest.Response);
        }

        return client;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    internal static Document<T>? ParseDocument<T>(DocumentReference reference, T? obj, Document<T>? document, ObjectEnumerator jsonElementEnumerator, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        object? parseJsonElement(JsonElement jsonElement, Type objType)
        {
            JsonConverter? jsonConverter = jsonSerializerOptions.Converters.FirstOrDefault(i => i.CanConvert(objType));

            object? obj = null;

            if (jsonConverter != null)
            {
                try
                {
                    obj = jsonElement.Deserialize(objType, jsonSerializerOptions);
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
                            obj = documentFieldValue.Deserialize(objType, jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "referenceValue":
                        if (documentFieldValue.ValueKind == JsonValueKind.String &&
                            objType == typeof(DocumentReference))
                        {
                            string? reference = documentFieldValue.Deserialize<string>(jsonSerializerOptions);
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
                        if (documentFieldValue.ValueKind == JsonValueKind.Object &&
                            objType.GetInterfaces().Any(i => i == typeof(IGeoPoint)))
                        {
                            double? latitude = default;
                            double? longitude = default;
                            foreach (var geoProperty in documentFieldValue.EnumerateObject())
                            {
                                if (geoProperty.Name == "latitude")
                                {
                                    latitude = geoProperty.Value.Deserialize<double>(jsonSerializerOptions);
                                }
                                else if (geoProperty.Name == "longitude")
                                {
                                    longitude = geoProperty.Value.Deserialize<double>(jsonSerializerOptions);
                                }
                            }

                            obj = Activator.CreateInstance(objType);

                            if (latitude.HasValue && longitude.HasValue)
                            {
                                objType.GetProperty(nameof(IGeoPoint.Latitude))?.SetValue(obj, latitude.Value);
                                objType.GetProperty(nameof(IGeoPoint.Longitude))?.SetValue(obj, longitude.Value);
                            }
                        }
                        break;
                    case "arrayValue":
                        if (documentFieldValue.ValueKind == JsonValueKind.Object &&
                            documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement arrayProperty &&
                            arrayProperty.ValueKind == JsonValueKind.Array &&
                            arrayProperty.EnumerateArray() is ArrayEnumerator arrayEnumerator)
                        {
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
                                    obj = Activator.CreateInstance(objType);

                                    if (obj != null)
                                    {
                                        Type[] collectionGenericArgsType = collectionInterfaceType.GetGenericArguments();

                                        parseCollectionFields(collectionInterfaceType, collectionGenericArgsType[0], obj, arrayEnumerator);
                                    }
                                }
                            }
                        }
                        break;
                    case "mapValue":
                        if (documentFieldValue.ValueKind == JsonValueKind.Object &&
                            documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement mapProperty &&
                            mapProperty.ValueKind == JsonValueKind.Object &&
                            mapProperty.EnumerateObject() is ObjectEnumerator mapEnumerator)
                        {
                            var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                            obj = Activator.CreateInstance(objType);

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
                object? parsedSubObj = parseJsonElement(fieldElement, valueType);

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
                    object? parsedSubObj = parseJsonElement(fieldElement, valueType);

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
            var keysProperty = dictionaryInterfaceType.GetProperty("Keys");
            var removeMethod = dictionaryInterfaceType.GetMethod("Remove");
            var keyParameter = new object?[1];

            if (itemProperty != null && keysProperty != null && removeMethod != null)
            {
                object? keys = keysProperty.GetValue(dictionaryObj, emptyParameterPlaceholder);

                IEnumerable? keysEnumerable = (IEnumerable?)keys;

                if (itemProperty == null || removeMethod == null || keysEnumerable == null)
                {
                    throw new Exception("Invalid dictionary type.");
                }

                List<object?> keysAdded = new();
                List<object> keysToRemove = new();

                foreach (var fieldProperty in enumerator)
                {
                    string? documentFieldKey = $"\"{fieldProperty.Name}\"";

                    object? objKey = JsonSerializer.Deserialize(
                        documentFieldKey,
                        keyType,
                        jsonSerializerOptions);

                    keyParameter[0] = objKey;

                    object? parsedSubObj = parseJsonElement(fieldProperty.Value, valueType);

                    itemProperty.SetValue(dictionaryObj, parsedSubObj, keyParameter);

                    keysAdded.Add(objKey);
                }

                foreach (object key in keysEnumerable)
                {
                    if (!keysAdded.Contains(key))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (object key in keysToRemove)
                {
                    keyParameter[0] = key;
                    removeMethod.Invoke(dictionaryObj, keyParameter);
                }
            }
        }

        bool checkProperty(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute, string name, bool onlyWithAttribute)
        {
            string? nameToCompare = null;
            bool isValueIncluded = false;

            if (!propertyInfo.CanWrite)
            {
                return false;
            }

            if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseValueAttribute)) is FirebaseValueAttribute firebaseValueAttribute)
            {
                nameToCompare = firebaseValueAttribute.Name;
                isValueIncluded = true;
            }
            else if (!onlyWithAttribute)
            {
                if (memberToCheckAttribute.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                {
                    nameToCompare = jsonPropertyNameAttribute.Name;
                }
                isValueIncluded = true;
            }

            if (!isValueIncluded)
            {
                return false;
            }

            if (nameToCompare == null || string.IsNullOrWhiteSpace(nameToCompare))
            {
                nameToCompare = jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return nameToCompare.Equals(name);
        }

        PropertyInfo? getEquivalentProperty(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, string name, bool onlyWithAttribute)
        {
            foreach (var propertyInfo in propertyInfos)
            {
                if (checkProperty(propertyInfo, propertyInfo, name, onlyWithAttribute))
                {
                    return propertyInfo;
                }
            }

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic)
                {
                    continue;
                }

                string propertyNameEquivalent = ClassFieldHelpers.GetPropertyName(fieldInfo);

                PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyNameEquivalent));

                if (propertyInfo == null)
                {
                    continue;
                }

                if (checkProperty(propertyInfo, fieldInfo, name, onlyWithAttribute))
                {
                    return propertyInfo;
                }
            }

            return null;
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, ObjectEnumerator enumerator)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            foreach (var firebaseField in enumerator)
            {
                var property = getEquivalentProperty(propertyInfos, fieldInfos, firebaseField.Name, includeOnlyWithAttribute);

                if (property == null)
                {
                    continue;
                }

                var subObjType = property.PropertyType;

                object? parsedSubObj = parseJsonElement(firebaseField.Value, subObjType);

                property.SetValue(obj, parsedSubObj);
            }
        }

        string? name = default;
        DateTimeOffset? createTime = default;
        DateTimeOffset? updateTime = default;
        Type objType = typeof(T);
        obj ??= document?.Model ?? Activator.CreateInstance<T>();

        if (document != null)
        {
            document.Reference = reference;
            document.Model = obj;
        }

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

        if (name != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            if (document == null)
            {
                document = new Document<T>(name, reference, obj, createTime.Value, updateTime.Value);
            }
            else
            {
                document.Name = name;
                document.CreateTime = createTime.Value;
                document.UpdateTime = updateTime.Value;
            }
        }

        return document;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
    internal static async Task<Stream> PopulateDocument<T>(FirebaseConfig config, T? obj, Document<T>? document, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObject(Type objType, object? obj, Action? onFirstAppend, Action? onPostAppend)
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteRawValue(serialized);
                }
                else
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("nullValue");
                    writer.WriteNullValue();
                    writer.WriteEndObject();
                }
            }
            else if (obj == null)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend?.Invoke();
                }
                writer.WriteStartObject();
                writer.WritePropertyName("nullValue");
                writer.WriteNullValue();
                writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("booleanValue");
                    writer.WriteStringValue(serialized);
                    writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("integerValue");
                    writer.WriteStringValue(serialized);
                    writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("doubleValue");
                    writer.WriteStringValue(serialized);
                    writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("stringValue");
                    writer.WriteStringValue(serialized);
                    writer.WriteEndObject();
                }
            }
            else if (objType.IsAssignableFrom(typeof(DateTime)))
            {
                string? serialized = null;
                try
                {
                    if (obj == null)
                    {
                        serialized = JsonSerializer.Serialize(new DateTime().ToUniversalTime(), jsonSerializerOptions);
                    }
                    else
                    {
                        serialized = JsonSerializer.Serialize(((DateTime)obj).ToUniversalTime(), jsonSerializerOptions);
                    }
                }
                catch { }
                if (serialized != null)
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("timestampValue");
                    writer.WriteRawValue(serialized);
                    writer.WriteEndObject();
                }
            }
            else if (objType.IsAssignableFrom(typeof(DateTimeOffset)))
            {
                string? serialized = null;
                try
                {
                    if (obj == null)
                    {
                        serialized = JsonSerializer.Serialize(new DateTime().ToUniversalTime(), jsonSerializerOptions);
                    }
                    else
                    {
                        serialized = JsonSerializer.Serialize(((DateTimeOffset)obj).UtcDateTime, jsonSerializerOptions);
                    }
                }
                catch { }
                if (serialized != null)
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("timestampValue");
                    writer.WriteRawValue(serialized);
                    writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("stringValue");
                    writer.WriteRawValue(serialized);
                    writer.WriteEndObject();
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
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("bytesValue");
                    writer.WriteRawValue(serialized);
                    writer.WriteEndObject();
                }
            }
            else if (obj is DocumentReference documentReference)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend?.Invoke();
                }
                writer.WriteStartObject();
                writer.WritePropertyName("referenceValue");
                writer.WriteStringValue(documentReference.BuildUrlCascade(config.ProjectId));
                writer.WriteEndObject();
            }
            else if (obj is IGeoPoint geoPoint)
            {
                if (!hasAppended)
                {
                    hasAppended = true;
                    onFirstAppend?.Invoke();
                }
                writer.WriteStartObject();
                writer.WritePropertyName("geoPointValue");
                writer.WriteStartObject();
                writer.WritePropertyName("latitude");
                writer.WriteNumberValue(geoPoint.Latitude);
                writer.WritePropertyName("longitude");
                writer.WriteNumberValue(geoPoint.Longitude);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            else if (objType.IsArray && objType.GetArrayRank() == 1 && objType.GetElementType() is Type elementType && obj is Array array)
            {
                if (array.Length == 0)
                {
                    if (!hasAppended)
                    {
                        hasAppended = true;
                        onFirstAppend?.Invoke();
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("arrayValue");
                    writer.WriteStartObject();
                    writer.WritePropertyName("values");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    parseArrayFields(elementType, array,
                        () =>
                        {
                            if (!hasAppended)
                            {
                                hasAppended = true;
                                onFirstAppend?.Invoke();
                            }
                            writer.WriteStartObject();
                            writer.WritePropertyName("arrayValue");
                            writer.WriteStartObject();
                            writer.WritePropertyName("values");
                            writer.WriteStartArray();
                        },
                        () =>
                        {
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                            writer.WriteEndObject();
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

                        parseDictionaryFields(dictionaryGenericCollectionArgsType[0], dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], (IEnumerable)obj,
                            () =>
                            {
                                if (!hasAppended)
                                {
                                    hasAppended = true;
                                    onFirstAppend?.Invoke();
                                }
                                writer.WriteStartObject();
                                writer.WritePropertyName("mapValue");
                                writer.WriteStartObject();
                                writer.WritePropertyName("fields");
                                writer.WriteStartObject();
                            },
                            () =>
                            {
                                writer.WriteEndObject();
                                writer.WriteEndObject();
                                writer.WriteEndObject();
                            });

                        break;
                    }

                    if (collectionInterfaceType != null)
                    {
                        Type[] collectionGenericArgsType = collectionInterfaceType.GetGenericArguments();

                        parseCollectionFields(collectionGenericArgsType[0], (IEnumerable)obj,
                            () =>
                            {
                                if (!hasAppended)
                                {
                                    hasAppended = true;
                                    onFirstAppend?.Invoke();
                                }
                                writer.WriteStartObject();
                                writer.WritePropertyName("arrayValue");
                                writer.WriteStartObject();
                                writer.WritePropertyName("values");
                                writer.WriteStartArray();
                            },
                            () =>
                            {
                                writer.WriteEndArray();
                                writer.WriteEndObject();
                                writer.WriteEndObject();
                            });

                        break;
                    }

                    parseObjectFields(objType, obj,
                        () =>
                        {
                            if (!hasAppended)
                            {
                                hasAppended = true;
                                onFirstAppend?.Invoke();
                            }
                            writer.WriteStartObject();
                            writer.WritePropertyName("mapValue");
                            writer.WriteStartObject();
                            writer.WritePropertyName("fields");
                            writer.WriteStartObject();
                        },
                        () =>
                        {
                            writer.WriteEndObject();
                            writer.WriteEndObject();
                            writer.WriteEndObject();
                        });

                    break;
                }
            }

            if (hasAppended)
            {
                onPostAppend?.Invoke();
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseArrayFields(Type elementType, Array arrayObj, Action? onFirstAppend, Action? onPostAppend)
        {
            bool hasAppended = false;
            foreach (var obj in arrayObj)
            {
                parseObject(elementType, obj, 
                    () =>
                    {
                        if (!hasAppended)
                        {
                            hasAppended = true;
                            onFirstAppend?.Invoke();
                        }
                    },
                    null);
            }
            if (hasAppended)
            {
                onPostAppend?.Invoke();
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseCollectionFields(Type elementType, IEnumerable collectionObj, Action? onFirstAppend, Action? onPostAppend)
        {
            bool hasAppended = false;
            foreach (var obj in collectionObj)
            {
                parseObject(elementType, obj,
                    () =>
                    {
                        if (!hasAppended)
                        {
                            hasAppended = true;
                            onFirstAppend?.Invoke();
                        }
                    },
                    null);
            }
            if (hasAppended)
            {
                onPostAppend?.Invoke();
            }
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseDictionaryFields(Type keyValuePairType, Type keyType, Type valueType, IEnumerable dictionaryObj, Action? onFirstAppend, Action? onPostAppend)
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
                    parseObject(valueType, value,
                        () =>
                        {
                            if (!hasAppended)
                            {
                                hasAppended = true;
                                onFirstAppend?.Invoke();
                            }
                            writer.WritePropertyName(key);
                        },
                        null);
                }
            }
            if (hasAppended)
            {
                onPostAppend?.Invoke();
            }
        }

        bool tryIfIncluded(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute, bool onlyWithAttribute, [MaybeNullWhen(false)] out string name)
        {
            name = null;
            bool returnValue = false;

            if (!propertyInfo.CanWrite)
            {
                return false;
            }

            if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseValueAttribute)) is FirebaseValueAttribute firebaseValueAttribute)
            {
                name = firebaseValueAttribute.Name;
                returnValue = true;
            }
            else if (!onlyWithAttribute)
            {
                if (memberToCheckAttribute.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                {
                    name = jsonPropertyNameAttribute.Name;
                }
                returnValue = true;
            }

            if (returnValue && string.IsNullOrWhiteSpace(name))
            {
                name = jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return returnValue;
        }

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, Action? onFirstAppend, Action? onPostAppend)
        {
            bool hasAppended = false;
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool onlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;
            List<string> includedNames = new();

            foreach (var propertyInfo in propertyInfos)
            {
                if (tryIfIncluded(propertyInfo, propertyInfo, onlyWithAttribute, out string? name))
                {
                    if (includedNames.Contains(name))
                    {
                        continue;
                    }

                    includedNames.Add(name);

                    object? propertyObj = propertyInfo.GetValue(obj);
                    parseObject(propertyInfo.PropertyType, propertyObj,
                        () =>
                        {
                            if (!hasAppended)
                            {
                                hasAppended = true;
                                onFirstAppend?.Invoke();
                            }
                            writer.WritePropertyName(name);
                        },
                        null);
                }
            }

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic)
                {
                    continue;
                }

                string propertyNameEquivalent = ClassFieldHelpers.GetPropertyName(fieldInfo);

                PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyNameEquivalent));

                if (propertyInfo == null)
                {
                    continue;
                }

                if (tryIfIncluded(propertyInfo, fieldInfo, onlyWithAttribute, out string? name))
                {
                    if (includedNames.Contains(name))
                    {
                        continue;
                    }

                    includedNames.Add(name);

                    object? propertyObj = propertyInfo.GetValue(obj);
                    parseObject(propertyInfo.PropertyType, propertyObj,
                        () =>
                        {
                            if (!hasAppended)
                            {
                                hasAppended = true;
                                onFirstAppend?.Invoke();
                            }
                            writer.WritePropertyName(name);
                        },
                        null);
                }
            }

            if (hasAppended)
            {
                onPostAppend?.Invoke();
            }
        }

        Type objType = typeof(T);
        obj ??= document?.Model;

        if (obj == null)
        {
            throw new ArgumentException($"Both {nameof(obj)} and {nameof(document)} is a null reference. Provide at least one to populate.");
        }

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
                () =>
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("fields");
                    writer.WriteStartObject();
                },
                () =>
                {
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                });
        }
        else
        {
            parseObjectFields(
                objType,
                obj,
                () =>
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("fields");
                    writer.WriteStartObject();
                },
                () =>
                {
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                });
        }

        await writer.FlushAsync();

        return stream;
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
}
