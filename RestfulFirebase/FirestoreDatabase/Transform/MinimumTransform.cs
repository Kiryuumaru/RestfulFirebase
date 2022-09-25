using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "minimum" transformation parameter for "minimum" transform commit writes.
/// </summary>
public class MinimumTransform : FieldTransform
{
    /// <summary>
    /// Creates field "minimum" transformation parameter for "minimum" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "minimum".
    /// </typeparam>
    /// <param name="minimumValue">
    /// The value to "minimum" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "minimum".
    /// </param>
    /// <returns>
    /// The created <see cref="MinimumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MinimumTransform Create<TModel>(object minimumValue, string[] propertyNamePath)
    {
        return new(minimumValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "minimum" transformation parameter for "minimum" transform commit writes.
    /// </summary>
    /// <param name="minimumValue">
    /// The value to "minimum" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "minimum".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "minimum".
    /// </param>
    /// <returns>
    /// The created <see cref="MinimumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MinimumTransform Create(object minimumValue, Type modelType, string[] propertyNamePath)
    {
        return new(minimumValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "minimum" to the given property path.
    /// </summary>
    public object MinimumValue { get; }

    internal MinimumTransform(object minimumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);

        MinimumValue = minimumValue;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        var documentFieldPath = ClassMemberHelpers.GetDocumentFieldPath(ModelType, null, PropertyNamePath, jsonSerializerOptions);
        var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

        NumberType paramNumberType = GetNumberType(MinimumValue.GetType());
        NumberType propertyNumberType = GetNumberType(lastDocumentFieldPath.Type);

        if (paramNumberType == NumberType.Double && propertyNumberType != NumberType.Double)
        {
            throw new ArgumentException($"Minimum type mismatch. \"{lastDocumentFieldPath.Type}\" cannot minimum with \"{MinimumValue.GetType()}\"");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("fieldPath");
        writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
        writer.WritePropertyName("minimum");
        writer.WriteStartObject();
        if (propertyNumberType == NumberType.Integer)
        {
            writer.WritePropertyName("integerValue");
        }
        else if (propertyNumberType == NumberType.Double)
        {
            writer.WritePropertyName("doubleValue");
        }
        else
        {
            throw new Exception("Minimum type is not supported.");
        }
        writer.WriteRawValue(JsonSerializer.Serialize(MinimumValue, jsonSerializerOptions));
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
