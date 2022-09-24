using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    [RequiresUnreferencedCode("Calls RestfulFirebase.FirestoreDatabase.Transform.FieldTransform.BuildUtf8JsonWriter(Utf8JsonWriter, FirebaseConfig, JsonSerializerOptions)")]
    internal void BuildAsUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("transform");
        writer.WriteStartObject();
        writer.WritePropertyName("document");
        writer.WriteStringValue(DocumentReference.BuildUrlCascade(config.ProjectId));
        writer.WritePropertyName("fieldTransforms");
        writer.WriteStartArray();
        foreach (var fieldTransform in FieldTransform.FieldTransforms)
        {
            fieldTransform.BuildUtf8JsonWriter(writer, config, jsonSerializerOptions);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
