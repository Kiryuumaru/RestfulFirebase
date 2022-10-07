using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
/// </summary>
public class AppendMissingElementsTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "appendMissingElements" to the given property path.
    /// </summary>
    public IEnumerable<object> AppendMissingElementsValue { get; }

    internal AppendMissingElementsTransform(IEnumerable<object> appendMissingElementsValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);

        AppendMissingElementsValue = appendMissingElementsValue;
    }
}
