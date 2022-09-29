using ObservableHelpers.ComponentModel;
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
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static System.Text.Json.JsonElement;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
[ObservableObject]
public partial class Document
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Document"/></para>
    /// <para><see cref="DocumentReference"/></para>
    /// <para><see cref="Document"/> array</para>
    /// <para><see cref="DocumentReference"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Document"/></para>
    /// <para><see cref="List{T}"/> with item <see cref="DocumentReference"/></para>
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of <see cref="Document"/>.
        /// </summary>
        public List<Document> Documents { get; } = new();

        /// <summary>
        /// Adds the document to the builder.
        /// </summary>
        /// <param name="document">
        /// The document to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is a null reference.
        /// </exception>
        public Builder Add(Document document)
        {
            ArgumentNullException.ThrowIfNull(document);

            Documents.Add(document);
            return this;
        }

        /// <summary>
        /// Adds the document to the builder.
        /// </summary>
        /// <param name="documentReference">
        /// The document with no model to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> is a null reference.
        /// </exception>
        public Builder Add(DocumentReference documentReference)
        {
            ArgumentNullException.ThrowIfNull(documentReference);

            Documents.Add(new Document(documentReference));
            return this;
        }

        /// <summary>
        /// Adds multiple the document to the builder.
        /// </summary>
        /// <param name="documents">
        /// The documents to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<Document> documents)
        {
            ArgumentNullException.ThrowIfNull(documents);

            Documents.AddRange(documents);
            return this;
        }

        /// <summary>
        /// Adds multiple the document to the builder.
        /// </summary>
        /// <param name="documentReferences">
        /// The documents with no model to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<DocumentReference> documentReferences)
        {
            ArgumentNullException.ThrowIfNull(documentReferences);

            Documents.AddRange(documentReferences.Select(i => new Document(i)));
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Document"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="document">
        /// The <see cref="Document"/> to convert.
        /// </param>
        public static implicit operator Builder(Document document)
        {
            return Create().Add(document);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReference">
        /// The <see cref="DocumentReference"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentReference documentReference)
        {
            return Create().Add(new Document(documentReference));
        }

        /// <summary>
        /// Converts the <see cref="Document"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="documents">
        /// The <see cref="Document"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(Document[] documents)
        {
            return Create().AddRange(documents);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReferences">
        /// The <see cref="DocumentReference"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentReference[] documentReferences)
        {
            return Create().AddRange(documentReferences);
        }

        /// <summary>
        /// Converts the <see cref="Document"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="documents">
        /// The <see cref="Document"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<Document> documents)
        {
            return Create().AddRange(documents);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReferences">
        /// The <see cref="DocumentReference"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<DocumentReference> documentReferences)
        {
            return Create().AddRange(documentReferences);
        }
    }

    #region Properties

    private static readonly object?[] emptyParameterPlaceholder = Array.Empty<object?>();

    /// <summary>
    /// Gets the name of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    string? name;

    /// <summary>
    /// Gets the reference of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DocumentReference reference;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> create time of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DateTimeOffset createTime;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> update time of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DateTimeOffset updateTime;

    #endregion

    #region Initializers

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static Document? Parse(
        DocumentReference? reference,
        Type objType,
        object? obj,
        Document? document,
        ObjectEnumerator jsonElementEnumerator,
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
                            obj = DocumentReference.Parse(documentFieldValue, jsonSerializerOptions);
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
                var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, null, propertyInfo.Name, jsonSerializerOptions);
                
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

        reference ??= DocumentReference.Parse(name);

        if (name != null &&
            reference != null &&
            createTime.HasValue &&
            updateTime.HasValue)
        {
            if (document == null)
            {
                Type genericDefinition = typeof(Document<>);
                Type genericType = genericDefinition.MakeGenericType(objType);
                document = (Document?)Activator.CreateInstance(genericType, new object?[] { reference, hasFields ? obj : null });

                if (document == null)
                {
                    throw new Exception($"Failed to create instance of {nameof(genericType)}");
                }
            }

            document.Name = name;
            document.Reference = reference;
            document.CreateTime = createTime.Value;
            document.UpdateTime = updateTime.Value;
        }

        return document;
    }

    /// <summary>
    /// Creates an instance of <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    public Document(DocumentReference reference)
    {
        this.reference = reference;
    }

    #endregion

    #region Methods

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

        ModelHelpers.BuildUtf8JsonWriter(config, writer, objType, obj, this, jsonSerializerOptions);
    }

    #endregion
}

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public partial class Document<T> : Document
     where T : class
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Document{T}"/></para>
    /// <para><see cref="DocumentReference"/></para>
    /// <para><see cref="Document{T}"/> array</para>
    /// <para><see cref="DocumentReference"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Document{T}"/></para>
    /// <para><see cref="List{T}"/> with item <see cref="DocumentReference"/></para>
    /// </summary>
    public new class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of <see cref="Document{T}"/>.
        /// </summary>
        public List<Document<T>> Documents { get; } = new();

        /// <summary>
        /// Adds the document to the builder.
        /// </summary>
        /// <param name="document">
        /// The document to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is a null reference.
        /// </exception>
        public Builder Add(Document<T> document)
        {
            ArgumentNullException.ThrowIfNull(document);

            Documents.Add(document);
            return this;
        }

        /// <summary>
        /// Adds the document to the builder.
        /// </summary>
        /// <param name="documentReference">
        /// The document with no model to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> is a null reference.
        /// </exception>
        public Builder Add(DocumentReference documentReference)
        {
            ArgumentNullException.ThrowIfNull(documentReference);

            Documents.Add(new Document<T>(documentReference, null));
            return this;
        }

        /// <summary>
        /// Adds multiple the document to the builder.
        /// </summary>
        /// <param name="documents">
        /// The documents to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<Document<T>> documents)
        {
            ArgumentNullException.ThrowIfNull(documents);

            Documents.AddRange(documents);
            return this;
        }

        /// <summary>
        /// Adds multiple the document to the builder.
        /// </summary>
        /// <param name="documentReferences">
        /// The documents with no model to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added document.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<DocumentReference> documentReferences)
        {
            ArgumentNullException.ThrowIfNull(documentReferences);

            Documents.AddRange(documentReferences.Select(i => new Document<T>(i, null)));
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Document{T}"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="document">
        /// The <see cref="Document{T}"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(Document<T> document)
        {
            return Create().Add(document);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReference">
        /// The <see cref="DocumentReference"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentReference documentReference)
        {
            return Create().Add(new Document<T>(documentReference, null));
        }

        /// <summary>
        /// Converts the <see cref="Document{T}"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="documents">
        /// The <see cref="Document{T}"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(Document<T>[] documents)
        {
            return Create().AddRange(documents);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReferences">
        /// The <see cref="DocumentReference"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentReference[] documentReferences)
        {
            return Create().AddRange(documentReferences);
        }

        /// <summary>
        /// Converts the <see cref="Document{T}"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="documents">
        /// The <see cref="Document{T}"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documents"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<Document<T>> documents)
        {
            return Create().AddRange(documents);
        }

        /// <summary>
        /// Converts the <see cref="DocumentReference"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentReferences">
        /// The <see cref="DocumentReference"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<DocumentReference> documentReferences)
        {
            return Create().AddRange(documentReferences);
        }
    }

    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> model of the document.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    T? model;

    #endregion

    #region Initializers

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static Document<T>? Parse(
        DocumentReference? reference,
        T? obj,
        Document? document,
        ObjectEnumerator jsonElementEnumerator,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Document? newDocument = Parse(reference, typeof(T), obj, document, jsonElementEnumerator, jsonSerializerOptions);
        if (newDocument is Document<T> typedDocument)
        {
            return typedDocument;
        }
        return null;
    }

    /// <summary>
    /// Creates an instance of <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    /// <param name="model">
    /// The model of the document.
    /// </param>
    public Document(DocumentReference reference, T? model)
        : base (reference)
    {
        this.model = model;
    }

    #endregion

    #region Methods

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

    #endregion
}
