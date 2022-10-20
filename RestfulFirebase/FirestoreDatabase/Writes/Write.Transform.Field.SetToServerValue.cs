using RestfulFirebase.FirestoreDatabase.Enums;
using System;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="SetToServerValueTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="documentFieldPath">
    /// The field path of the document field to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="SetToServerValueTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TWrite SetToServerRequestTime(params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new SetToServerValueTransform(ServerValue.RequestTime, documentFieldPath, false));

        return (TWrite)this;
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite, TModel>
{
    /// <summary>
    /// Adds new <see cref="SetToServerValueTransform"/> parameter to perform a transform operation.
    /// </summary>
    /// <param name="propertyPath">
    /// The property path of the model to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="SetToServerValueTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TWrite PropertySetToServerRequestTime(params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        GetLastDocumentTransform().WritableFieldTransforms.Add(new SetToServerValueTransform(ServerValue.RequestTime, propertyPath, true));

        return (TWrite)this;
    }
}

/// <summary>
/// The field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
/// </summary>
public class SetToServerValueTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "setToServerValue" to the given property path.
    /// </summary>
    public ServerValue ServerValue { get; }

    internal SetToServerValueTransform(ServerValue serverValue, string[] namePath, bool isPathPropertyName)
        : base(namePath, isPathPropertyName)
    {
        ServerValue = serverValue;
    }
}
