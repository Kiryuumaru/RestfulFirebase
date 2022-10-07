﻿using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace RestfulFirebase.FirestoreDatabase.Models;

public partial class Document
{
    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static Document? Parse(
        FirebaseApp app,
        DocumentReference? reference,
        Type? objType,
        object? obj,
        Document? document,
        JsonElement.ObjectEnumerator jsonElementEnumerator,
        JsonSerializerOptions jsonSerializerOptions)
    {
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
                            obj = DocumentReference.Parse(app, documentFieldValue, jsonSerializerOptions);
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
                            arrayProperty.ValueKind == JsonValueKind.Array)
                        {
                            if (objType.IsArray && objType.GetArrayRank() == 1)
                            {
                                Type? arrayElementType = objType.GetElementType();

                                if (arrayElementType != null)
                                {
                                    obj = parseArrayFields(arrayElementType, arrayProperty);
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

                                        parseCollectionFields(collectionInterfaceType, collectionGenericArgsType[0], obj, arrayProperty);
                                    }
                                }
                            }
                        }
                        break;
                    case "mapValue":
                        if (documentFieldValue.ValueKind == JsonValueKind.Object &&
                            documentFieldValue.EnumerateObject().FirstOrDefault().Value is JsonElement mapProperty &&
                            mapProperty.ValueKind == JsonValueKind.Object)
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

                                    parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, mapProperty);
                                }
                                else
                                {
                                    parseObjectFields(objType, obj, mapProperty);
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
        object? parseArrayFields(Type valueType, JsonElement element)
        {
            List<object?> items = new();

            foreach (var fieldElement in element.EnumerateArray())
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
        void parseCollectionFields(Type collectionInterfaceType, Type valueType, object collectionObj, JsonElement element)
        {
            var addMethod = collectionInterfaceType.GetMethod("Add");
            var clearMethod = collectionInterfaceType.GetMethod("Clear");
            var addMethodParameter = new object?[1];

            if (addMethod != null && clearMethod != null)
            {
                clearMethod.Invoke(collectionObj, emptyParameterPlaceholder);

                foreach (var fieldElement in element.EnumerateArray())
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
        void parseDictionaryFields(Type dictionaryInterfaceType, Type keyType, Type valueType, object dictionaryObj, JsonElement element)
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

                foreach (var fieldProperty in element.EnumerateObject())
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
        void parseObjectFields(Type objType, object obj, JsonElement element)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            foreach (var propertyInfo in propertyInfos)
            {
                var documentField = DocumentFieldHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyInfo.Name, jsonSerializerOptions);
                
                if (documentField == null)
                {
                    continue;
                }

                if (!element.TryGetProperty(documentField.DocumentFieldName, out JsonElement documentFieldElement) &&
                    documentFieldElement.ValueKind == JsonValueKind.Undefined)
                {
                    continue;
                }

                object? parsedSubObj = parseJsonElement(documentFieldElement, documentField.Type);

                propertyInfo.SetValue(obj, parsedSubObj);
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
                        continue;
                    }

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

                        parseDictionaryFields(dictionaryInterfaceType, dictionaryGenericArgsType[0], dictionaryGenericArgsType[1], obj, documentProperty.Value);
                    }
                    else
                    {
                        parseObjectFields(objType, obj, documentProperty.Value);
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
        }

        return document;
    }

    internal virtual object? GetModel()
    {
        return null;
    }

    internal virtual void SetModel(object? obj)
    {
        return;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal void BuildUtf8JsonWriter(FirebaseConfig config, Utf8JsonWriter writer, JsonSerializerOptions? jsonSerializerOptions)
    {
        object? obj = GetModel();

        if (obj == null)
        {
            throw new ArgumentException($"Model is a null reference. Provide a model to build to writer.");
        }

        Type objType = obj.GetType();

        ModelBuilderHelpers.BuildUtf8JsonWriter(config, writer, objType, obj, this, jsonSerializerOptions);
    }
}

public partial class Document<T> : Document
     where T : class
{
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static Document<T>? Parse(
        FirebaseApp app,
        DocumentReference? reference,
        T? obj,
        Document? document,
        JsonElement.ObjectEnumerator jsonElementEnumerator,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Document? newDocument = Parse(app, reference, typeof(T), obj, document, jsonElementEnumerator, jsonSerializerOptions);
        if (newDocument is Document<T> typedDocument)
        {
            return typedDocument;
        }
        return null;
    }

    internal override object? GetModel()
    {
        return Model;
    }

    internal override void SetModel(object? obj)
    {
        if (obj == null)
        {
            Model = null;
        }
        else if (obj is T typedObj)
        {
            Model = typedObj;
        }
        else
        {
            throw new ArgumentException($"Mismatch type of {nameof(obj)} and {typeof(T)}");
        }
    }
}