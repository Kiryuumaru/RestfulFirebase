using System;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "minimum" transformation parameter for "minimum" transform commit writes.
/// </summary>
public class MinimumTransform : FieldTransform
{
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
}
