using System;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "increment" transformation parameter for "increment" transform commit writes.
/// </summary>
public class IncrementTransform : FieldTransform
{
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
}
