using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field minimum transformation parameter for minimum transform commit writes.
/// </summary>
public class FieldTransformMinimum : FieldTransform
{
    /// <summary>
    /// Gets the object to minimum to the given property path.
    /// </summary>
    public object MinimumValue { get; }

    internal FieldTransformMinimum(object minimumValue, Type modelType, string[] propertyNamePath)
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
