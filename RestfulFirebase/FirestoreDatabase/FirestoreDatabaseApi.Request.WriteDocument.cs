using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transform;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Http;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.Collections;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse> ExecuteWriteDocument(IEnumerable<Document>? patchDocuments = default, IEnumerable<DocumentReference>? deleteDocumentReferences = default, IEnumerable<DocumentTransform>? transformDocuments = default, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        string url =
            $"{FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":commit")}";

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("writes");
        writer.WriteStartArray();
        if (patchDocuments != null)
        {
            foreach (var document in patchDocuments)
            {
                if (document.GetModel() is object obj)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(App.Config.ProjectId));
                    writer.WritePropertyName("fields");
                    ModelBuilderHelpers.BuildUtf8JsonWriter(App.Config, writer, obj.GetType(), obj, document, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(App.Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
        }
        if (deleteDocumentReferences != null)
        {
            foreach (var deleteDocumentReference in deleteDocumentReferences)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(deleteDocumentReference.BuildUrlCascade(App.Config.ProjectId));
                writer.WriteEndObject();
            }
        }
        if (transformDocuments != null)
        {
            foreach (var documentTransform in transformDocuments)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("transform");
                writer.WriteStartObject();
                writer.WritePropertyName("document");
                writer.WriteStringValue(documentTransform.DocumentReference.BuildUrlCascade(App.Config.ProjectId));
                writer.WritePropertyName("fieldTransforms");
                writer.WriteStartArray();
                foreach (var fieldTransform in documentTransform.FieldTransforms)
                {
                    var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(fieldTransform.ModelType, fieldTransform.PropertyNamePath, jsonSerializerOptions);
                    var lastDocumentFieldPath = documentFieldPath.LastOrDefault()!;

                    switch (fieldTransform)
                    {
                        case AppendMissingElementsTransform appendMissingElementsTransform:

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("appendMissingElements");
                            writer.WriteStartObject();
                            writer.WritePropertyName("values");
                            writer.WriteStartArray();
                            foreach (var obj in appendMissingElementsTransform.AppendMissingElementsValue)
                            {
                                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, obj?.GetType(), obj, jsonSerializerOptions, null, null);
                            }
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                            writer.WriteEndObject();

                            break;
                        case IncrementTransform incrementTransform:

                            Type incrementValueType = incrementTransform.IncrementValue.GetType();

                            NumberType incrementParamNumberType = NumberTypeHelpers.GetNumberType(incrementValueType);
                            NumberType incrementPropertyNumberType = NumberTypeHelpers.GetNumberType(lastDocumentFieldPath.Type);

                            if (incrementParamNumberType == NumberType.Double && incrementPropertyNumberType != NumberType.Double)
                            {
                                throw new ArgumentException($"Increment type mismatch. \"{lastDocumentFieldPath.Type}\" cannot increment with \"{incrementValueType}\"");
                            }

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("increment");
                            writer.WriteStartObject();
                            if (incrementPropertyNumberType == NumberType.Integer)
                            {
                                writer.WritePropertyName("integerValue");
                            }
                            else if (incrementPropertyNumberType == NumberType.Double)
                            {
                                writer.WritePropertyName("doubleValue");
                            }
                            else
                            {
                                throw new Exception("Increment type is not supported.");
                            }
                            writer.WriteRawValue(JsonSerializer.Serialize(incrementTransform.IncrementValue, jsonSerializerOptions));
                            writer.WriteEndObject();
                            writer.WriteEndObject();

                            break;
                        case MaximumTransform maximumTransform:

                            Type maximumValueType = maximumTransform.MaximumValue.GetType();

                            NumberType maximumParamNumberType = NumberTypeHelpers.GetNumberType(maximumValueType);
                            NumberType maximumPropertyNumberType = NumberTypeHelpers.GetNumberType(lastDocumentFieldPath.Type);

                            if (maximumParamNumberType == NumberType.Double && maximumPropertyNumberType != NumberType.Double)
                            {
                                throw new ArgumentException($"Maximum type mismatch. \"{lastDocumentFieldPath.Type}\" cannot maximum with \"{maximumValueType}\"");
                            }

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("maximum");
                            writer.WriteStartObject();
                            if (maximumPropertyNumberType == NumberType.Integer)
                            {
                                writer.WritePropertyName("integerValue");
                            }
                            else if (maximumPropertyNumberType == NumberType.Double)
                            {
                                writer.WritePropertyName("doubleValue");
                            }
                            else
                            {
                                throw new Exception("Maximum type is not supported.");
                            }
                            writer.WriteRawValue(JsonSerializer.Serialize(maximumTransform.MaximumValue, jsonSerializerOptions));
                            writer.WriteEndObject();
                            writer.WriteEndObject();

                            break;
                        case MinimumTransform minimumTransform:

                            Type minimumValueType = minimumTransform.MinimumValue.GetType();

                            NumberType minimumParamNumberType = NumberTypeHelpers.GetNumberType(minimumValueType);
                            NumberType minimumPropertyNumberType = NumberTypeHelpers.GetNumberType(lastDocumentFieldPath.Type);

                            if (minimumParamNumberType == NumberType.Double && minimumPropertyNumberType != NumberType.Double)
                            {
                                throw new ArgumentException($"Minimum type mismatch. \"{lastDocumentFieldPath.Type}\" cannot minimum with \"{minimumValueType}\"");
                            }

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("minimum");
                            writer.WriteStartObject();
                            if (minimumPropertyNumberType == NumberType.Integer)
                            {
                                writer.WritePropertyName("integerValue");
                            }
                            else if (minimumPropertyNumberType == NumberType.Double)
                            {
                                writer.WritePropertyName("doubleValue");
                            }
                            else
                            {
                                throw new Exception("Minimum type is not supported.");
                            }
                            writer.WriteRawValue(JsonSerializer.Serialize(minimumTransform.MinimumValue, jsonSerializerOptions));
                            writer.WriteEndObject();
                            writer.WriteEndObject();

                            break;
                        case RemoveAllFromArrayTransform removeAllFromArrayTransform:

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("removeAllFromArray");
                            writer.WriteStartObject();
                            writer.WritePropertyName("values");
                            writer.WriteStartArray();
                            foreach (var obj in removeAllFromArrayTransform.RemoveAllFromArrayValue)
                            {
                                ModelBuilderHelpers.BuildUtf8JsonWriterObject(App.Config, writer, obj?.GetType(), obj, jsonSerializerOptions, null, null);
                            }
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                            writer.WriteEndObject();

                            break;
                        case SetToServerValueTransform setToServerValueTransform:

                            writer.WriteStartObject();
                            writer.WritePropertyName("fieldPath");
                            writer.WriteStringValue(string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName)));
                            writer.WritePropertyName("setToServerValue");
                            writer.WriteStringValue(setToServerValueTransform.ServerValue.ToEnumString());
                            writer.WriteEndObject();

                            break;
                        default:
                            throw new NotImplementedException($"{fieldTransform.GetType()} Field transform is not implemented.");
                    }
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
        }
        writer.WriteEndArray();
        if (transaction != null)
        {
            BuildTransaction(writer, transaction);
        }
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await ExecutePost(authorization, stream, url, cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse> WriteDocument(IEnumerable<Document>? patchDocuments = default, IEnumerable<DocumentReference>? deleteDocumentReferences = default, IEnumerable<DocumentTransform>? transformDocuments = default, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        if (patchDocuments == null &&
            deleteDocumentReferences == null &&
            transformDocuments == null)
        {
            throw new ArgumentException($"All " +
                $"\"{nameof(patchDocuments)}\", " +
                $"\"{nameof(deleteDocumentReferences)}\" and " +
                $"\"{nameof(transformDocuments)}\" are null references. Provide at least one argument.");
        }

        return await ExecuteWriteDocument(patchDocuments, deleteDocumentReferences, transformDocuments, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a patch operation to document.
    /// </summary>
    /// <param name="patchDocument">
    /// The requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="patchDocument"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> PatchDocument(Document patchDocument, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patchDocument);

        return WriteDocument(new Document[] { patchDocument }, null, null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a patch operation to document.
    /// </summary>
    /// <param name="patchDocuments">
    /// The requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="patchDocuments"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> PatchDocuments(IEnumerable<Document> patchDocuments, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patchDocuments);

        return WriteDocument(patchDocuments, null, null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a delete operation to documents.
    /// </summary>
    /// <param name="deleteDocument">
    /// The requested <see cref="Document{T}"/> to delete the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="deleteDocument"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> DeleteDocument(Document deleteDocument, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deleteDocument);

        return WriteDocument(null, new DocumentReference[] { deleteDocument.Reference }, null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a delete operation to documents.
    /// </summary>
    /// <param name="deleteDocuments">
    /// The requested <see cref="Document{T}"/> to delete the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="deleteDocuments"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> DeleteDocuments(IEnumerable<Document> deleteDocuments, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deleteDocuments);

        return WriteDocument(null, deleteDocuments.Select(i => i.Reference), null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }
}
