using System;

namespace RestfulFirebase.FirestoreDatabase.Transforms;

/// <summary>
/// The field "maximum" transformation parameter for "maximum" transform commit writes.
/// </summary>
public class MaximumTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "maximum" to the given property path.
    /// </summary>
    public object MaximumValue { get; }

    /// <summary>
    /// Creates new instance of <see cref="MaximumTransform"/>.
    /// </summary>
    /// <param name="maximumValue">
    /// The value to "maximum" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "maximum".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "maximum".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="maximumValue"/>,
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public MaximumTransform(object maximumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);

        MaximumValue = maximumValue;
    }
}
