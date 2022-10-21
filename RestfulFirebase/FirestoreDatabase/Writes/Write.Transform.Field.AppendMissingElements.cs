using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementsValue">
    /// The values to apply append missing elements to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite AppendMissingElements(IEnumerable<object> appendMissingElementsValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(appendMissingElementsValue, documentFieldPath, false));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite AppendMissingElements(object appendMissingElementValue, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue }, documentFieldPath, false));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue1">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="appendMissingElementValue2">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue1"/>,
    /// <paramref name="appendMissingElementValue2"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite AppendMissingElements(object appendMissingElementValue1, object appendMissingElementValue2, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue1);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue2);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue1, appendMissingElementValue2 }, documentFieldPath, false));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue1">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="appendMissingElementValue2">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="appendMissingElementValue3">
    /// The value to apply append missing element to the document field.
    /// </param>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue1"/>,
    /// <paramref name="appendMissingElementValue2"/>,
    /// <paramref name="appendMissingElementValue3"/> or
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    public TWrite AppendMissingElements(object appendMissingElementValue1, object appendMissingElementValue2, object appendMissingElementValue3, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue1);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue2);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue3);
        ArgumentNullException.ThrowIfNull(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue1, appendMissingElementValue2, appendMissingElementValue3 }, documentFieldPath, false));

        return (TWrite)this;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementsValue">
    /// The values to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementsValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyAppendMissingElements(IEnumerable<object> appendMissingElementsValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementsValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(appendMissingElementsValue, propertyPath, true));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyAppendMissingElements(object appendMissingElementValue, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue }, propertyPath, true));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue1">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="appendMissingElementValue2">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue1"/>,
    /// <paramref name="appendMissingElementValue2"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyAppendMissingElements(object appendMissingElementValue1, object appendMissingElementValue2, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue1);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue2);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue1, appendMissingElementValue2 }, propertyPath, true));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds new <see cref="AppendMissingElementsTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="appendMissingElementValue1">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="appendMissingElementValue2">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="appendMissingElementValue3">
    /// The value to apply append missing elements to the property of the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="AppendMissingElementsTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="appendMissingElementValue1"/>,
    /// <paramref name="appendMissingElementValue2"/>,
    /// <paramref name="appendMissingElementValue3"/> or
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    public TWrite PropertyAppendMissingElements(object appendMissingElementValue1, object appendMissingElementValue2, object appendMissingElementValue3, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(appendMissingElementValue1);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue2);
        ArgumentNullException.ThrowIfNull(appendMissingElementValue3);
        ArgumentNullException.ThrowIfNull(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new AppendMissingElementsTransform(new object[] { appendMissingElementValue1, appendMissingElementValue2, appendMissingElementValue3 }, propertyPath, true));

        return (TWrite)this;
    }
}

/// <summary>
/// The field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes.
/// </summary>
public class AppendMissingElementsTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "appendMissingElements" to the given property path.
    /// </summary>
    public IEnumerable<object> AppendMissingElementsValue { get; }

    internal AppendMissingElementsTransform(IEnumerable<object> appendMissingElementsValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        AppendMissingElementsValue = appendMissingElementsValue;
    }
}
