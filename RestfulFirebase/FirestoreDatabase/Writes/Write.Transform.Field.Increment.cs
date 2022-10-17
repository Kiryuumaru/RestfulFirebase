using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="IncrementTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="incrementValue">
    /// The value to apply increment to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="IncrementTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite Increment(object incrementValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new IncrementTransform(incrementValue, documentFieldPath, false));

        return (TWrite)this;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="IncrementTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="incrementValue">
    /// The value to apply increment to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="IncrementTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyIncrement(object incrementValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new IncrementTransform(incrementValue, propertyPath, true));

        return (TWrite)this;
    }
}

/// <summary>
/// The field "increment" transformation parameter for "increment" transform commit writes.
/// </summary>
public class IncrementTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "increment" to the given property path.
    /// </summary>
    public object IncrementValue { get; }

    internal IncrementTransform(object incrementValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);

        IncrementValue = incrementValue;
    }
}
