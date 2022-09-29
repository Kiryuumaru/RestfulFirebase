using System;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "maximum" transformation parameter for "maximum" transform commit writes.
/// </summary>
public class MaximumTransform : FieldTransform
{
    /// <summary>
    /// Creates field "maximum" transformation parameter for "maximum" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "maximum".
    /// </typeparam>
    /// <param name="maximumValue">
    /// The value to "maximum" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "maximum".
    /// </param>
    /// <returns>
    /// The created <see cref="MaximumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="maximumValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MaximumTransform Create<TModel>(object maximumValue, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(maximumValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "maximum" transformation parameter for "maximum" transform commit writes.
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
    /// <returns>
    /// The created <see cref="MaximumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="maximumValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MaximumTransform Create(object maximumValue, Type modelType, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(maximumValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "maximum" to the given property path.
    /// </summary>
    public object MaximumValue { get; }

    internal MaximumTransform(object maximumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        MaximumValue = maximumValue;
    }
}
