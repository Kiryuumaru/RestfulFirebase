using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
}

public partial class FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <param name="documentReference">
    /// The document reference of the document to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReference"/> is a null reference.
    /// </exception>
    public WriteWithDocumentTransform Transform(DocumentReference documentReference)
    {
        ArgumentNullException.ThrowIfNull(documentReference);

        TWrite write = (TWrite)Clone();

        DocumentTransform documentTransform = new(App, null, documentReference);

        write.WritableTransformDocuments.Add(documentTransform);

        return new(write, true);
    }

    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model of the document to transform.
    /// </typeparam>
    /// <param name="documentReference">
    /// The document reference of the document to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReference"/> is a null reference.
    /// </exception>
    public WriteWithDocumentTransform<TModel> Transform<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(DocumentReference documentReference)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(documentReference);

        TWrite write = (TWrite)Clone();

        DocumentTransform documentTransform = new(App, typeof(TModel), documentReference);

        write.WritableTransformDocuments.Add(documentTransform);

        return new(write, true);
    }
}

public partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
{
    internal DocumentTransform GetLastDocumentTransform()
    {
        return TransformDocuments.Last();
    }
}

/// <summary>
/// The document transform parameter for write commits.
/// </summary>
public class DocumentTransform
{
    /// <summary>
    /// Gets the <see cref="References.DocumentReference"/> to transform.
    /// </summary>
    public DocumentReference DocumentReference { get; }

    /// <summary>
    /// Gets the type of the model to transform.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? ModelType { get; }

    /// <summary>
    /// Gets the <see cref="FieldTransform"/> for document field transforms.
    /// </summary>
    public IReadOnlyList<FieldTransform> FieldTransforms { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal readonly List<FieldTransform> WritableFieldTransforms;

    internal DocumentTransform(FirebaseApp app, Type? modelType, DocumentReference documentReference)
    {
        App = app;
        ModelType = modelType;
        DocumentReference = documentReference;

        WritableFieldTransforms = new();
        FieldTransforms = WritableFieldTransforms.AsReadOnly();
    }
}
