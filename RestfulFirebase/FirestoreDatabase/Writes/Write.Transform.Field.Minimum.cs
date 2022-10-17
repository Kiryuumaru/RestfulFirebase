using System;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="MinimumTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="minimumValue">
    /// The value to apply minimum to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="MinimumTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite Minimum(object minimumValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new MinimumTransform(minimumValue, documentFieldPath, false));

        return (TWrite)this;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="MinimumTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="minimumValue">
    /// The value to apply minimum to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="MinimumTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyMinimum(object minimumValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new MinimumTransform(minimumValue, propertyPath, true));

        return (TWrite)this;
    }
}

/// <summary>
/// The field "minimum" transformation parameter for "minimum" transform commit writes.
/// </summary>
public class MinimumTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "minimum" to the given property path.
    /// </summary>
    public object MinimumValue { get; }

    internal MinimumTransform(object minimumValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);

        MinimumValue = minimumValue;
    }
}
