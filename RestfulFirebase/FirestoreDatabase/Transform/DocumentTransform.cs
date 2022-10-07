using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transform;

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
    /// Gets the <see cref="FieldTransform"/> for document field transforms.
    /// </summary>
    public IReadOnlyList<FieldTransform> FieldTransforms { get; }

    private readonly List<FieldTransform> fieldTransforms;

    internal DocumentTransform(DocumentReference documentReference)
    {
        ArgumentNullException.ThrowIfNull(documentReference);

        DocumentReference = documentReference;

        fieldTransforms = new();
        FieldTransforms = fieldTransforms.AsReadOnly();
    }
}
