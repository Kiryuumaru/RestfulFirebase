using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The document transform parameter for write commits.
/// </summary>
public class DocumentTransform
{
    /// <summary>
    /// The builder for <see cref="DocumentTransform"/>.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of built <see cref="DocumentTransform"/>.
        /// </summary>
        public List<DocumentTransform> DocumentTransforms { get; } = new();

        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <param name="documentReference">
        /// The <see cref="References.DocumentReference"/> to build the <see cref="DocumentTransform"/>.
        /// </param>
        /// <param name="fieldTransform">
        /// The <see cref="References.DocumentReference"/> to build the <see cref="DocumentTransform"/>.
        /// </param>
        public Builder Add(DocumentReference documentReference, FieldTransform.Builder fieldTransform)
        {
            DocumentTransform documentTransform = new(documentReference, fieldTransform);
            DocumentTransforms.Add(documentTransform);
            return this;
        }

    }

    /// <summary>
    /// Gets the <see cref="References.DocumentReference"/> to transform.
    /// </summary>
    public DocumentReference DocumentReference { get; }

    /// <summary>
    /// Gets the <see cref="FieldTransform"/> builders for document field transforms.
    /// </summary>
    public FieldTransform.Builder FieldTransform { get; }

    internal DocumentTransform(DocumentReference documentReference, FieldTransform.Builder fieldTransform)
    {
        DocumentReference = documentReference;
        FieldTransform = fieldTransform;
    }
}
