using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes.
/// </summary>
public class RemoveAllFromArrayTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "removeAllFromArray" to the given property path.
    /// </summary>
    public IEnumerable<object> RemoveAllFromArrayValue { get; }

    /// <summary>
    /// Creates new instance of <see cref="RemoveAllFromArrayTransform"/>.
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeAllFromArrayValue"/>,
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public RemoveAllFromArrayTransform(IEnumerable<object> removeAllFromArrayValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);

        RemoveAllFromArrayValue = removeAllFromArrayValue;
    }
}
