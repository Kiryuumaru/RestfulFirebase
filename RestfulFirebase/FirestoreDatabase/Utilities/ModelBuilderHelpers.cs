using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;
using System.Collections;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Utilities;

internal static class ModelBuilderHelpers
{
    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static Document? Parse(
        FirebaseApp app,
        DocumentReference? reference,
        Type? objType,
        object? obj,
        Document? document,
        JsonElement.ObjectEnumerator jsonElementEnumerator,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Dictionary<string, object?> documentFields = new();

        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
        object? parseJsonElement(JsonElement jsonElement, string fieldName, Type? objType)
        {
            object? obj = null;

            if (objType == null)
            {
                var documentField = jsonElement.EnumerateObject().FirstOrDefault();
                string documentFieldType = documentField.Name;
                JsonElement documentFieldValue = documentField.Value;
                switch (documentFieldType)
                {
                    case "nullValue":
                        break;
                    case "booleanValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<bool>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "integerValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<long>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "doubleValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<double>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "timestampValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<DateTimeOffset>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "stringValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<string>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "bytesValue":
                        try
                        {
                            obj = documentFieldValue.Deserialize<byte[]>(jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "referenceValue":
                        try
                        {
                            obj = DocumentReference.Parse(app, documentFieldValue, jsonSerializerOptions);
                        }
                        catch { }
                        break;
                    case "geoPointValue":
                        try
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

                            if (latitude.HasValue && longitude.HasValue)
                            {
                                obj = new GeoPoint()
                                {
                                    Latitude = latitude.Value,
                                    Longitude = longitude.Value
                                };
                            }
                        }
                        catch { }
                        break;
                    case "arrayValue":
                        if (documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement arrayProperty)
                        {
                            obj = parseArrayFields(null, fieldName, arrayProperty);
                        }
                        break;
                    case "mapValue":
                        if (documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement mapProperty)
                        {
                            parseObjectFields(objType, obj, fieldName, mapProperty);
                        }
                        break;
                }
            }
            else if (jsonSerializerOptions.Converters.Any(i => i.CanConvert(objType)))
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
                        if (objType == typeof(DocumentReference))
                        {
                            obj = DocumentReference.Parse(app, documentFieldValue, jsonSerializerOptions);
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
                        if (documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement arrayProperty)
                        {
                            if (objType.IsArray && objType.GetArrayRank() == 1)
                            {
                                Type? arrayElementType = objType.GetElementType();

                                if (arrayElementType != null)
                                {
                                    obj = parseArrayFields(arrayElementType, fieldName, arrayProperty);
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

                                        parseCollectionFields(collectionInterfaceType, collectionGenericArgsType[0], obj, fieldName, arrayProperty);
                                    }
                                }
                            }
                        }
                        break;
                    case "mapValue":
                        if (documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement mapProperty)
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

                                    parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, fieldName, mapProperty);
                                }
                                else
                                {
                                    parseObjectFields(objType, obj, fieldName, mapProperty);
                                }
                            }
                        }
                        break;
                }
            }

            return obj;
        }

        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
        object? parseArrayFields(Type? valueType, string fieldName, JsonElement element)
        {
            List<object?> items = new();

            if (element.ValueKind == JsonValueKind.Array)
            {
                int index = 0;

                foreach (var fieldElement in element.EnumerateArray())
                {
                    string subFieldName = string.IsNullOrEmpty(fieldName) ? index.ToString() : $"{fieldName}.{index}";
                    object? parsedSubObj = parseJsonElement(fieldElement, subFieldName, valueType);

                    documentFields.Add(subFieldName, parsedSubObj);

                    items.Add(parsedSubObj);

                    index++;
                }
            }

            Array obj;
            if (valueType == null)
            {
                obj = Array.CreateInstance(typeof(object), items.Count);
            }
            else
            {
                obj = Array.CreateInstance(valueType, items.Count);
            }

            for (int i = 0; i < items.Count; i++)
            {
                obj.SetValue(items[i], i);
            }

            return obj;
        }

        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
        void parseCollectionFields(Type collectionInterfaceType, Type valueType, object collectionObj, string fieldName, JsonElement element)
        {
            var addMethod = collectionInterfaceType.GetMethod("Add");
            var clearMethod = collectionInterfaceType.GetMethod("Clear");
            var addMethodParameter = new object?[1];

            if (addMethod != null && clearMethod != null)
            {
                clearMethod.Invoke(collectionObj, emptyParameterPlaceholder);

                if (element.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;

                    foreach (var fieldElement in element.EnumerateArray())
                    {
                        string subFieldName = string.IsNullOrEmpty(fieldName) ? index.ToString() : $"{fieldName}.{index}";
                        object? parsedSubObj = parseJsonElement(fieldElement, subFieldName, valueType);

                        documentFields.Add(subFieldName, parsedSubObj);

                        addMethodParameter[0] = parsedSubObj;

                        addMethod.Invoke(collectionObj, addMethodParameter);

                        index++;
                    }
                }
            }
        }

        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
        void parseDictionaryFields(Type dictionaryInterfaceType, Type keyType, Type valueType, object dictionaryObj, string fieldName, JsonElement element)
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

                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var fieldProperty in element.EnumerateObject())
                    {
                        string? documentFieldKey = $"\"{fieldProperty.Name}\"";

                        object? objKey = JsonSerializer.Deserialize(
                            documentFieldKey,
                            keyType,
                            jsonSerializerOptions);

                        keyParameter[0] = objKey;

                        string subFieldName = string.IsNullOrEmpty(fieldName) ? fieldProperty.Name : $"{fieldName}.{fieldProperty.Name}";
                        object? parsedSubObj = parseJsonElement(fieldProperty.Value, subFieldName, valueType);

                        documentFields.Add(subFieldName, parsedSubObj);

                        itemProperty.SetValue(dictionaryObj, parsedSubObj, keyParameter);

                        keysAdded.Add(objKey);
                    }
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

        [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
        void parseObjectFields(Type? objType, object? obj, string fieldName, JsonElement element)
        {
            List<string> alreadyAdded = new();

            if (objType != null)
            {
                PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

                foreach (var propertyInfo in propertyInfos)
                {
                    var documentField = ModelFieldHelpers.GetModelField(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyInfo.Name, jsonSerializerOptions);

                    if (documentField == null)
                    {
                        continue;
                    }

                    if (!element.TryGetProperty(documentField.ModelFieldName, out JsonElement documentFieldElement) &&
                        documentFieldElement.ValueKind == JsonValueKind.Undefined)
                    {
                        continue;
                    }

                    string subFieldName = string.IsNullOrEmpty(fieldName) ? documentField.ModelFieldName : $"{fieldName}.{documentField.ModelFieldName}";
                    object? parsedSubObj = parseJsonElement(documentFieldElement, subFieldName, documentField.Type);

                    documentFields.Add(subFieldName, parsedSubObj);

                    propertyInfo.SetValue(obj, parsedSubObj);

                    alreadyAdded.Add(documentField.ModelFieldName);
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (alreadyAdded.Contains(property.Name))
                {
                    continue;
                }

                string subFieldName = string.IsNullOrEmpty(fieldName) ? property.Name : $"{fieldName}.{property.Name}";
                object? parsedSubObj = parseJsonElement(property.Value, subFieldName, null);

                documentFields.Add(subFieldName, parsedSubObj);
            }
        }

        string? name = default;
        DateTimeOffset? createTime = default;
        DateTimeOffset? updateTime = default;
        bool hasFields = false;

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
                    if (objType == null)
                    {
                        parseObjectFields(objType, obj, "", documentProperty.Value);
                    }
                    else
                    {
                        hasFields = true;

                        obj ??= document?.GetModel() ?? Activator.CreateInstance(objType);

                        if (obj == null)
                        {
                            throw new Exception($"Failed to create instance of {nameof(objType)}");
                        }

                        document?.SetModel(obj);

                        var dictionaryInterfaceType = objType.GetInterfaces().FirstOrDefault(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                        if (dictionaryInterfaceType != null)
                        {
                            Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();

                            parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, "", documentProperty.Value);
                        }
                        else
                        {
                            parseObjectFields(objType, obj, "", documentProperty.Value);
                        }
                    }

                    break;
            }
        }

        if (!hasFields)
        {
            document?.SetModel(null);
        }

        reference ??= DocumentReference.Parse(app, name);

        if (name != null &&
            reference != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            if (document == null)
            {
                if (objType == null)
                {
                    document = new(reference);
                }
                else
                {
                    Type genericDefinition = typeof(Document<>);
                    Type genericType = genericDefinition.MakeGenericType(objType);
                    document = (Document?)Activator.CreateInstance(genericType, new object?[] { reference, hasFields ? obj : null });

                    if (document == null)
                    {
                        throw new Exception($"Failed to create instance of {nameof(genericType)}");
                    }
                }
            }

            document.Name = name;
            document.Reference = reference;
            document.CreateTime = createTime.Value;
            document.UpdateTime = updateTime.Value;

            List<string> fieldsToRemove = new(documentFields.Keys);

            foreach (var pair in documentFields)
            {
                document.WritableFields.AddOrUpdate(pair.Key, pair.Value, (_, _) => pair.Value);
                fieldsToRemove.Remove(pair.Key);
            }

            foreach (var fieldToRemove in fieldsToRemove)
            {
                document.WritableFields.TryRemove(fieldToRemove, out _);
            }
        }

        return document;
    }

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static Document<TModel>? Parse<TModel>(
        FirebaseApp app,
        DocumentReference? reference,
        object? obj,
        Document? document,
        JsonElement.ObjectEnumerator jsonElementEnumerator,
        JsonSerializerOptions jsonSerializerOptions)
        where TModel : class
    {
        Document? newDocument = Parse(app, reference, typeof(TModel), obj, document, jsonElementEnumerator, jsonSerializerOptions);
        if (newDocument is Document<TModel> typedDocument)
        {
            return typedDocument;
        }
        return null;
    }

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriterObject(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type? objType,
        object? obj,
        JsonSerializerOptions? jsonSerializerOptions,
        Action? onFirstAppend,
        Action? onPostAppend)
    {
        bool hasAppended = false;

        if (objType == typeof(object))
        {
            objType = obj?.GetType();
        }

        if (objType == null || obj == null)
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
        else if (jsonSerializerOptions?.Converters.FirstOrDefault(i => i.CanConvert(objType)) is JsonConverter jsonConverter)
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
        else if (
            objType.IsAssignableFrom(typeof(string)) ||
            objType.IsAssignableFrom(typeof(char)))
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
                BuildUtf8JsonWriterArrayFields(config, writer, elementType, array, jsonSerializerOptions,
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

                    BuildUtf8JsonWriterDictionaryFields(config, writer, dictionaryGenericCollectionArgsType[0], dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], (IEnumerable)obj, jsonSerializerOptions,
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

                    BuildUtf8JsonWriterCollectionFields(config, writer, collectionGenericArgsType[0], (IEnumerable)obj, jsonSerializerOptions,
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

                BuildUtf8JsonWriterObjectFields(config, writer, objType, obj, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriterArrayFields(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type elementType,
        Array arrayObj,
        JsonSerializerOptions? jsonSerializerOptions,
        Action? onFirstAppend,
        Action? onPostAppend)
    {
        bool hasAppended = false;
        foreach (var obj in arrayObj)
        {
            BuildUtf8JsonWriterObject(config, writer, elementType, obj, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriterCollectionFields(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type elementType,
        IEnumerable collectionObj,
        JsonSerializerOptions? jsonSerializerOptions,
        Action? onFirstAppend,
        Action? onPostAppend)
    {
        bool hasAppended = false;
        foreach (var obj in collectionObj)
        {
            BuildUtf8JsonWriterObject(config, writer, elementType, obj, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriterDictionaryFields(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type keyValuePairType,
        Type keyType,
        Type valueType,
        IEnumerable dictionaryObj,
        JsonSerializerOptions? jsonSerializerOptions,
        Action? onFirstAppend,
        Action? onPostAppend)
    {
        if (keyType != null)
        {

        }

        bool hasAppended = false;
        var keyProperty = keyValuePairType.GetProperty("Key");
        var valueProperty = keyValuePairType.GetProperty("Value");

        foreach (var obj in dictionaryObj)
        {
            string? key = keyProperty?.GetValue(obj)?.ToString();
            object? value = valueProperty?.GetValue(obj);
            if (key != null)
            {
                BuildUtf8JsonWriterObject(config, writer, valueType, value, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriterObjectFields(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type objType,
        object obj,
        JsonSerializerOptions? jsonSerializerOptions,
        Action? onFirstAppend,
        Action? onPostAppend)
    {
        bool tryIfIncluded(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute, bool onlyWithAttribute, [MaybeNullWhen(false)] out string name)
        {
            name = null;
            bool returnValue = false;

            // Special exclude (for ObservableHelpers)
            if (propertyInfo.Name == "SyncOperation" ||
                propertyInfo.Name == "SynchronizePropertyChangedEvent" ||
                propertyInfo.Name == "SynchronizePropertyChangingEvent")
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
                name = jsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return returnValue;
        }

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
                BuildUtf8JsonWriterObject(config, writer, propertyInfo.PropertyType, propertyObj, jsonSerializerOptions,
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
                BuildUtf8JsonWriterObject(config, writer, propertyInfo.PropertyType, propertyObj, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriter(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        Type objType,
        object? obj,
        Document? document,
        JsonSerializerOptions? jsonSerializerOptions)
    {
        obj ??= document?.GetModel();

        if (obj == null)
        {
            ArgumentException.Throw($"Both {nameof(obj)} and {nameof(document)} is a null reference. Provide at least one to populate.");
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

            BuildUtf8JsonWriterDictionaryFields(config, writer, dictionaryGenericCollectionArgsType[0], dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], (IEnumerable)obj, jsonSerializerOptions,
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
            BuildUtf8JsonWriterObjectFields(config, writer, objType, obj, jsonSerializerOptions,
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

    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal static void BuildUtf8JsonWriter<T>(
        FirebaseConfig config,
        Utf8JsonWriter writer,
        T? obj,
        Document? document,
        JsonSerializerOptions? jsonSerializerOptions)
    {
        BuildUtf8JsonWriter(config, writer, typeof(T), obj, document, jsonSerializerOptions);
    }
}
