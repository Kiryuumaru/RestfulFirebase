using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes.
/// </summary>
public class RemoveAllFromArrayTransform : FieldTransform
{
    /// <summary>
    /// Creates field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "removeAllFromArray".
    /// </typeparam>
    /// <param name="removeAllFromArrayValue">
    /// The value to "removeAllFromArray" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "removeAllFromArray".
    /// </param>
    /// <returns>
    /// The created <see cref="RemoveAllFromArrayTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeAllFromArrayValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static RemoveAllFromArrayTransform Create<TModel>(IEnumerable<object> removeAllFromArrayValue, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(removeAllFromArrayValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes.
    /// </summary>
    /// <param name="removeAllFromArrayValue">
    /// The value to "removeAllFromArray" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "removeAllFromArray".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "removeAllFromArray".
    /// </param>
    /// <returns>
    /// The created <see cref="RemoveAllFromArrayTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeAllFromArrayValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static RemoveAllFromArrayTransform Create(IEnumerable<object> removeAllFromArrayValue, Type modelType, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(removeAllFromArrayValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "removeAllFromArray" to the given property path.
    /// </summary>
    public IEnumerable<object> RemoveAllFromArrayValue { get; }

    internal RemoveAllFromArrayTransform(IEnumerable<object> removeAllFromArrayValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        RemoveAllFromArrayValue = removeAllFromArrayValue;
    }
}
