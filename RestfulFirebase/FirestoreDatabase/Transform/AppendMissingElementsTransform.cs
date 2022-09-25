using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
/// </summary>
public class AppendMissingElementsTransform : FieldTransform
{
    /// <summary>
    /// Creates field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "appendMissingElements".
    /// </typeparam>
    /// <param name="appendMissingElementsValue">
    /// The value to "appendMissingElements" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "appendMissingElements".
    /// </param>
    /// <returns>
    /// The created <see cref="AppendMissingElementsTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static AppendMissingElementsTransform Create<TModel>(IEnumerable<object> appendMissingElementsValue, string[] propertyNamePath)
    {
        return new(appendMissingElementsValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
    /// </summary>
    /// <param name="appendMissingElementsValue">
    /// The value to "appendMissingElements" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "appendMissingElements".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "appendMissingElements".
    /// </param>
    /// <returns>
    /// The created <see cref="AppendMissingElementsTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static AppendMissingElementsTransform Create(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
    {
        return new(appendMissingElementsValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "appendMissingElements" to the given property path.
    /// </summary>
    public IEnumerable<object> AppendMissingElementsValue { get; }

    internal AppendMissingElementsTransform(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);

        AppendMissingElementsValue = appendMissingElementsValue;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        var documentFieldPath = ClassMemberHelpers.GetDocumentFieldPath(ModelType, null, PropertyNamePath, jsonSerializerOptions);
        var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

        writer.WriteStartObject();
        writer.WritePropertyName("fieldPath");
        writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
        writer.WritePropertyName("appendMissingElements");
        writer.WriteStartObject();
        writer.WritePropertyName("values");
        writer.WriteStartArray();
        foreach (var obj in AppendMissingElementsValue)
        {
            ModelHelpers.BuildUtf8JsonWriterObject(config, writer, obj?.GetType(), obj, jsonSerializerOptions, null, null);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
