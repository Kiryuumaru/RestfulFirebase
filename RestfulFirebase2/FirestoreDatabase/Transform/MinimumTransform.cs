using System;

namespace RestfulFirebase.FirestoreDatabase.Transforms;

/// <summary>
/// The field "minimum" transformation parameter for "minimum" transform commit writes.
/// </summary>
public class MinimumTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "minimum" to the given property path.
    /// </summary>
    public object MinimumValue { get; }

    /// <summary>
    /// Creates new instance of <see cref="MinimumTransform"/>.
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/>,
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public MinimumTransform(object minimumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);

        MinimumValue = minimumValue;
    }
}
