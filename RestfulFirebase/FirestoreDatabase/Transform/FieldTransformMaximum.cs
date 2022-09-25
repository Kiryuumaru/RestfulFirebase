using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field maximum transformation parameter for maximum transform commit writes.
/// </summary>
public class FieldTransformMaximum : FieldTransform
{
    /// <summary>
    /// Gets the object to maximum to the given property path.
    /// </summary>
    public object MaximumValue { get; }

    internal FieldTransformMaximum(object maximumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);

        MaximumValue = maximumValue;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal override void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        var documentFieldPath = ClassMemberHelpers.GetDocumentFieldPath(ModelType, null, PropertyNamePath, jsonSerializerOptions);
        var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

        NumberType paramNumberType = GetNumberType(MaximumValue.GetType());
        NumberType propertyNumberType = GetNumberType(lastDocumentFieldPath.Type);

        if (paramNumberType == NumberType.Double && propertyNumberType != NumberType.Double)
        {
            throw new ArgumentException($"Maximum type mismatch. \"{lastDocumentFieldPath.Type}\" cannot maximum with \"{MaximumValue.GetType()}\"");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("fieldPath");
        writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
        writer.WritePropertyName("maximum");
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
            throw new Exception("Maximum type is not supported.");
        }
        writer.WriteRawValue(JsonSerializer.Serialize(MaximumValue, jsonSerializerOptions));
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
