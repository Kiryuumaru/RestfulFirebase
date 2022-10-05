using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;
using ObservableHelpers.ComponentModel;
using System.Collections;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Utilities;

internal static class ModelBuilderHelpers
{
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
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
