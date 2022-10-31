using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeAllFromArrayValue">
    /// The values to apply remove from arrays to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeAllFromArrayValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite RemoveAllFromArray(IEnumerable<object> removeAllFromArrayValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(removeAllFromArrayValue, documentFieldPath, false));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite RemoveAllFromArray(object removeFromArrayValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue }, documentFieldPath, false));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue1">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="removeFromArrayValue2">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue1"/>,
    /// <paramref name="removeFromArrayValue2"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite RemoveAllFromArray(object removeFromArrayValue1, object removeFromArrayValue2, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue1);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue2);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue1, removeFromArrayValue2 }, documentFieldPath, false));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue1">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="removeFromArrayValue2">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="removeFromArrayValue3">
    /// The value to apply remove from array to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue1"/>,
    /// <paramref name="removeFromArrayValue2"/>,
    /// <paramref name="removeFromArrayValue3"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite RemoveAllFromArray(object removeFromArrayValue1, object removeFromArrayValue2, object removeFromArrayValue3, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue1);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue2);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue3);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue1, removeFromArrayValue2, removeFromArrayValue3 }, documentFieldPath, false));

        return write;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeAllFromArrayValue">
    /// The values to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeAllFromArrayValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyRemoveAllFromArray(IEnumerable<object> removeAllFromArrayValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(removeAllFromArrayValue, propertyPath, true));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyRemoveAllFromArray(object removeFromArrayValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue }, propertyPath, true));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue1">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="removeFromArrayValue2">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue1"/>,
    /// <paramref name="removeFromArrayValue2"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyRemoveAllFromArray(object removeFromArrayValue1, object removeFromArrayValue2, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue1);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue2);
        ArgumentNullException.ThrowIfNull(propertyPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue1, removeFromArrayValue2 }, propertyPath, true));

        return write;
    }

    /// <summary>
    /// Adds new <see cref="RemoveAllFromArrayTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="removeFromArrayValue1">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="removeFromArrayValue2">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="removeFromArrayValue3">
    /// The value to apply remove from arrays to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="RemoveAllFromArrayTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="removeFromArrayValue1"/>,
    /// <paramref name="removeFromArrayValue2"/>,
    /// <paramref name="removeFromArrayValue3"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyRemoveAllFromArray(object removeFromArrayValue1, object removeFromArrayValue2, object removeFromArrayValue3, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(removeFromArrayValue1);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue2);
        ArgumentNullException.ThrowIfNull(removeFromArrayValue3);
        ArgumentNullException.ThrowIfNull(propertyPath);

        TWrite write = (TWrite)Clone();

        write.GetLastDocumentTransform().WritableFieldTransforms.Add(new RemoveAllFromArrayTransform(new object[] { removeFromArrayValue1, removeFromArrayValue2, removeFromArrayValue3 }, propertyPath, true));

        return write;
    }
}

/// <summary>
/// The field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes.
/// </summary>
public class RemoveAllFromArrayTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "removeAllFromArray" to the given property path.
    /// </summary>
    public IEnumerable<object> RemoveAllFromArrayValue { get; }

    internal RemoveAllFromArrayTransform(IEnumerable<object> removeAllFromArrayValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        ArgumentNullException.ThrowIfNull(removeAllFromArrayValue);

        RemoveAllFromArrayValue = removeAllFromArrayValue;
    }
}
