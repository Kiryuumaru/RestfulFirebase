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

    internal RemoveAllFromArrayTransform(IEnumerable<object> removeAllFromArrayValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);

        RemoveAllFromArrayValue = removeAllFromArrayValue;
    }
}
