using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field increment transformation parameter for increment transform commit writes.
/// </summary>
public class FieldTransformIncrement : FieldTransform
{
    /// <summary>
    /// Gets the object to increment to the given property path.
    /// </summary>
    public object IncrementValue { get; }

    internal FieldTransformIncrement(object incrementValue, Type modelType, string[] propertyNamePath)
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

        IncrementType paramIncrementType = GetIncrementType(IncrementValue.GetType());
        IncrementType propertyIncrementType = GetIncrementType(lastDocumentFieldPath.Type);

        if (paramIncrementType != propertyIncrementType)
        {
            throw new ArgumentException($"Increment type mismatch. \"{lastDocumentFieldPath.Type}\" cannot increment with \"{IncrementValue.GetType()}\"");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("fieldPath");
        writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
        writer.WritePropertyName("increment");
        writer.WriteStartObject();
        if (paramIncrementType == IncrementType.Integer)
        {
            writer.WritePropertyName("integerValue");
        }
        else if (paramIncrementType == IncrementType.Double)
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

    internal static IncrementType GetIncrementType(Type incrementType)
    {
        if (incrementType.IsAssignableFrom(typeof(sbyte)) ||
            incrementType.IsAssignableFrom(typeof(byte)) ||
            incrementType.IsAssignableFrom(typeof(short)) ||
            incrementType.IsAssignableFrom(typeof(ushort)) ||
            incrementType.IsAssignableFrom(typeof(int)) ||
            incrementType.IsAssignableFrom(typeof(uint)) ||
            incrementType.IsAssignableFrom(typeof(long)) ||
            incrementType.IsAssignableFrom(typeof(ulong)) ||
            incrementType.IsAssignableFrom(typeof(nint)) ||
            incrementType.IsAssignableFrom(typeof(nuint)))
        {
            return IncrementType.Integer;
        }
        else if (
            incrementType.IsAssignableFrom(typeof(float)) ||
            incrementType.IsAssignableFrom(typeof(double)))
        {
            return IncrementType.Double;
        }
        else if (
            incrementType.IsAssignableFrom(typeof(decimal)))
        {
            throw new ArgumentException("Decimal increment is not yet supported.");
        }
        else
        {
            throw new ArgumentException($"\"{incrementType}\" type increment is not supported.");
        }
    }
}
