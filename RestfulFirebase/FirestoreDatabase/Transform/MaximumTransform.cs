using System;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "maximum" transformation parameter for "maximum" transform commit writes.
/// </summary>
public class MaximumTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "maximum" to the given property path.
    /// </summary>
    public object MaximumValue { get; }

    internal MaximumTransform(object maximumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);

        MaximumValue = maximumValue;
    }
}
