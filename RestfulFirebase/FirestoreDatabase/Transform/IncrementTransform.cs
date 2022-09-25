using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "increment" transformation parameter for "increment" transform commit writes.
/// </summary>
public class IncrementTransform : FieldTransform
{
    /// <summary>
    /// Creates field "increment" transformation parameter for "increment" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "increment".
    /// </typeparam>
    /// <param name="incrementValue">
    /// The value to "increment" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "increment".
    /// </param>
    /// <returns>
    /// The created <see cref="IncrementTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static IncrementTransform Create<TModel>(object incrementValue, string[] propertyNamePath)
    {
        return new(incrementValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "increment" transformation parameter for "increment" transform commit writes.
    /// </summary>
    /// <param name="incrementValue">
    /// The value to "increment" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "increment".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "increment".
    /// </param>
    /// <returns>
    /// The created <see cref="IncrementTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static IncrementTransform Create(object incrementValue, Type modelType, string[] propertyNamePath)
    {
        return new(incrementValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "increment" to the given property path.
    /// </summary>
    public object IncrementValue { get; }

    internal IncrementTransform(object incrementValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);

        IncrementValue = incrementValue;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        var documentFieldPath = ClassMemberHelpers.GetDocumentFieldPath(ModelType, null, PropertyNamePath, jsonSerializerOptions);
        var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

        NumberType paramNumberType = GetNumberType(IncrementValue.GetType());
        NumberType propertyNumberType = GetNumberType(lastDocumentFieldPath.Type);

        if (paramNumberType == NumberType.Double && propertyNumberType != NumberType.Double)
        {
            throw new ArgumentException($"Increment type mismatch. \"{lastDocumentFieldPath.Type}\" cannot increment with \"{IncrementValue.GetType()}\"");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("fieldPath");
        writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
        writer.WritePropertyName("increment");
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
            throw new Exception("Increment type is not supported.");
        }
        writer.WriteRawValue(JsonSerializer.Serialize(IncrementValue, jsonSerializerOptions));
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
