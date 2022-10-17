using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Transforms;

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
        private readonly List<DocumentTransform> documentTransforms = new();

        /// <summary>
        /// Gets the list of <see cref="DocumentTransform"/>.
        /// </summary>
        public IReadOnlyList<DocumentTransform> DocumentTransforms { get; }

        internal Builder()
        {
            DocumentTransforms = documentTransforms.AsReadOnly();
        }

        /// <summary>
        /// Adds new field transform to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="documentReference">
        /// The <see cref="References.DocumentReference"/> to transform.
        /// </param>
        /// <param name="fieldTransform">
        /// The <see cref="FieldTransform.Builder"/> builders for document field transforms.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> or
        /// <paramref name="fieldTransform"/> is a null reference.
        /// </exception>
        public Builder Add(DocumentReference documentReference, FieldTransform.Builder fieldTransform)
        {
            DocumentTransform documentTransform = new(documentReference, fieldTransform);
            documentTransforms.Add(documentTransform);
            return this;
        }

        /// <summary>
        /// Adds new instance of <see cref="DocumentTransform"/> to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="documentTransform">
        /// The <see cref="DocumentTransform"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTransform"/> is a null reference.
        /// </exception>
        public Builder Add(DocumentTransform documentTransform)
        {
            ArgumentNullException.ThrowIfNull(documentTransform);

            documentTransforms.Add(documentTransform);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="DocumentTransform"/> to the builder.
        /// </summary>
        /// <param name="documentTransforms">
        /// The multiple of <see cref="DocumentTransform"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTransforms"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<DocumentTransform> documentTransforms)
        {
            ArgumentNullException.ThrowIfNull(documentTransforms);

            this.documentTransforms.AddRange(documentTransforms);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="DocumentTransform"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentTransform">
        /// The <see cref="DocumentTransform"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTransform"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentTransform documentTransform)
        {
            return new Builder().Add(documentTransform);
        }

        /// <summary>
        /// Converts the <see cref="DocumentTransform"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentTransforms">
        /// The <see cref="DocumentTransform"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTransforms"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(DocumentTransform[] documentTransforms)
        {
            return new Builder().AddRange(documentTransforms);
        }

        /// <summary>
        /// Converts the <see cref="DocumentTransform"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="documentTransforms">
        /// The <see cref="DocumentTransform"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTransforms"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<DocumentTransform> documentTransforms)
        {
            return new Builder().AddRange(documentTransforms);
        }
    }

    /// <inheritdoc cref="Builder.Add(DocumentReference, FieldTransform.Builder)"/>
    public static Builder Add(DocumentReference documentReference, FieldTransform.Builder fieldTransform)
    {
        return new Builder().Add(documentReference, fieldTransform);
    }

    /// <inheritdoc cref="Builder.Add(DocumentTransform)"/>
    public static Builder Add(DocumentTransform documentTransform)
    {
        return new Builder().Add(documentTransform);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{DocumentTransform})"/>
    public static Builder AddRange(IEnumerable<DocumentTransform> documentTransforms)
    {
        return new Builder().AddRange(documentTransforms);
    }

    /// <summary>
    /// Gets the <see cref="References.DocumentReference"/> to transform.
    /// </summary>
    public DocumentReference DocumentReference { get; }

    /// <summary>
    /// Gets the <see cref="FieldTransform.Builder"/> builders for document field transforms.
    /// </summary>
    public FieldTransform.Builder FieldTransform { get; }

    /// <summary>
    /// Creates new instance of <see cref="DocumentTransform"/>.
    /// </summary>
    /// <param name="documentReference">
    /// The <see cref="References.DocumentReference"/> to transform.
    /// </param>
    /// <param name="fieldTransform">
    /// The <see cref="FieldTransform.Builder"/> builders for document field transforms.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReference"/> or
    /// <paramref name="fieldTransform"/> is a null reference.
    /// </exception>
    public DocumentTransform(DocumentReference documentReference, FieldTransform.Builder fieldTransform)
    {
        ArgumentNullException.ThrowIfNull(documentReference);
        ArgumentNullException.ThrowIfNull(fieldTransform);

        DocumentReference = documentReference;
        FieldTransform = fieldTransform;
    }
}
