using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ObservableHelpers.ComponentModel;
using RestfulFirebase.Authentication.Models;
using static System.Text.Json.JsonElement;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// The base implementation for all firebase cloud firestore request.
/// </summary>
public abstract class FirestoreDatabaseRequest<TResponse> : TransactionRequest<TResponse>, IAuthenticatedTransactionRequest
    where TResponse : TransactionResponse
{
    /// <inheritdoc/>
    public FirebaseUser? FirebaseUser { get; set; }

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();

    internal override async Task<HttpClient> GetClient()
    {
        var client = HttpClient ?? new HttpClient();

        if (FirebaseUser != null)
        {
            var tokenRequest = await Api.Authentication.GetFreshToken(this);
            tokenRequest.ThrowIfErrorOrEmptyResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenRequest.Result.IdToken);
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
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static CollectionReference? ParseCollectionReference(string? json)
    {
        if (json != null && !string.IsNullOrEmpty(json))
        {
            string[] paths = json.Split('/');
            object currentPath = Api.FirestoreDatabase.Collection(paths[5]);

            for (int i = 6; i < paths.Length; i++)
            {
                if (currentPath is CollectionReference colPath)
                {
                    currentPath = colPath.Document(paths[i]);
                }
                else if (currentPath is DocumentReference docPath)
                {
                    currentPath = docPath.Collection(paths[i]);
                }
            }

            if (currentPath is CollectionReference collectionReference)
            {
                return collectionReference;
            }
        }

        return null;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static CollectionReference? ParseCollectionReference(JsonElement jsonElement, JsonSerializerOptions jsonSerializerOptions)
    {
        return ParseCollectionReference(jsonElement.Deserialize<string>(jsonSerializerOptions));
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static DocumentReference? ParseDocumentReference(string? json)
    {
        if (json != null && !string.IsNullOrEmpty(json))
        {
            string[] paths = json.Split('/');
            object currentPath = Api.FirestoreDatabase.Collection(paths[5]);

            for (int i = 6; i < paths.Length; i++)
            {
                if (currentPath is CollectionReference colPath)
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
                return documentReference;
            }
        }

        return null;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static DocumentReference? ParseDocumentReference(JsonElement jsonElement, JsonSerializerOptions jsonSerializerOptions)
    {
        return ParseDocumentReference(jsonElement.Deserialize<string>(jsonSerializerOptions));
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static Document<T>? ParseDocument<T>(DocumentReference? reference, T? obj, Document<T>? document, ObjectEnumerator jsonElementEnumerator, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
                            obj = ParseDocumentReference(documentFieldValue, jsonSerializerOptions);
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
        void parseObjectFields(Type objType, object obj, ObjectEnumerator enumerator)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            foreach (var firebaseField in enumerator)
            {
                var propertyInfo = ClassMemberHelpers.GetPropertyInfo(propertyInfos, fieldInfos, includeOnlyWithAttribute, firebaseField.Name, jsonNamingPolicy);

                if (propertyInfo == null)
                {
                    continue;
                }

                var subObjType = propertyInfo.PropertyType;

                object? parsedSubObj = parseJsonElement(firebaseField.Value, subObjType);

                propertyInfo.SetValue(obj, parsedSubObj);
            }
        }

        string? name = default;
        DateTimeOffset? createTime = default;
        DateTimeOffset? updateTime = default;
        Type objType = typeof(T);
        obj ??= document?.Model ?? Activator.CreateInstance<T>();

        if (document != null)
        {
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

        reference ??= ParseDocumentReference(name);

        if (name != null &&
            reference != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            if (document != null)
            {
                document.Name = name;
                document.Reference = reference;
                document.CreateTime = createTime.Value;
                document.UpdateTime = updateTime.Value;
            }
            else
            {
                document = new Document<T>(name, reference, obj, createTime.Value, updateTime.Value);
            }
        }

        return document;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static void PopulateDocument<T>(FirebaseConfig config, Utf8JsonWriter writer, T? obj, Document<T>? document, JsonSerializerOptions jsonSerializerOptions)
        where T : class
    {
        JsonNamingPolicy? jsonNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy ?? DefaultJsonSerializerOption.PropertyNamingPolicy;

#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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

            // Special exclude
            if (propertyInfo.Name == nameof(ObservableObject.SyncOperation) ||
                propertyInfo.Name == nameof(ObservableObject.SynchronizePropertyChangedEvent) ||
                propertyInfo.Name == nameof(ObservableObject.SynchronizePropertyChangingEvent))
            {
                return false;
            }

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
        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
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

                string propertyNameEquivalent = ClassMemberHelpers.GetPropertyName(fieldInfo);

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
                },
                () =>
                {
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
                },
                () =>
                {
                    writer.WriteEndObject();
                });
        }
    }
}
