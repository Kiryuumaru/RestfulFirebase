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

    /// <summary>
    /// Creates new instance of <see cref="IncrementTransform"/>.
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/>,
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public IncrementTransform(object incrementValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);

        IncrementValue = incrementValue;
    }
}
