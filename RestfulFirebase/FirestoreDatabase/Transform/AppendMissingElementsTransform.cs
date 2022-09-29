using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
/// </summary>
public class AppendMissingElementsTransform : FieldTransform
{
    /// <summary>
    /// Creates field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "appendMissingElements".
    /// </typeparam>
    /// <param name="appendMissingElementsValue">
    /// The value to "appendMissingElements" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "appendMissingElements".
    /// </param>
    /// <returns>
    /// The created <see cref="AppendMissingElementsTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static AppendMissingElementsTransform Create<TModel>(IEnumerable<object> appendMissingElementsValue, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(appendMissingElementsValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
    /// </summary>
    /// <param name="appendMissingElementsValue">
    /// The value to "appendMissingElements" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "appendMissingElements".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "appendMissingElements".
    /// </param>
    /// <returns>
    /// The created <see cref="AppendMissingElementsTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static AppendMissingElementsTransform Create(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(appendMissingElementsValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "appendMissingElements" to the given property path.
    /// </summary>
    public IEnumerable<object> AppendMissingElementsValue { get; }

    internal AppendMissingElementsTransform(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        AppendMissingElementsValue = appendMissingElementsValue;
    }
}
