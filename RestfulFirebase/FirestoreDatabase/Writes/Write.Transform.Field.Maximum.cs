namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="MaximumTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="maximumValue">
    /// The value to apply maximum to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="MaximumTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="maximumValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite Maximum(object maximumValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new MaximumTransform(maximumValue, documentFieldPath, false));

        return (TWrite)this;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="MaximumTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="maximumValue">
    /// The value to apply maximum to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="MaximumTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="maximumValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyMaximum(object maximumValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new MaximumTransform(maximumValue, propertyPath, true));

        return (TWrite)this;
    }
}

/// <summary>
/// The field "maximum" transformation parameter for "maximum" transform commit writes.
/// </summary>
public class MaximumTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "maximum" to the given property path.
    /// </summary>
    public object MaximumValue { get; }

    internal MaximumTransform(object maximumValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        ArgumentNullException.ThrowIfNull(maximumValue);

        MaximumValue = maximumValue;
    }
}
