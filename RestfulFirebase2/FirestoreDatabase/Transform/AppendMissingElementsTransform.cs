using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transforms;

/// <summary>
/// The field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
/// </summary>
public class AppendMissingElementsTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "appendMissingElements" to the given property path.
    /// </summary>
    public IEnumerable<object> AppendMissingElementsValue { get; }

    /// <summary>
    /// Creates new instance of <see cref="AppendMissingElementsTransform"/>.
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/>,
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public AppendMissingElementsTransform(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);

        AppendMissingElementsValue = appendMissingElementsValue;
    }
}
