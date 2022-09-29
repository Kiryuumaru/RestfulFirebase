using System;
using System.Text.Json;
using RestfulFirebase.Common.Requests;
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

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class WriteDocumentRequest : FirestoreDatabaseRequest<TransactionResponse<WriteDocumentRequest>>
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to patch the document fields. If <see cref="Document{T}.Model"/> is a null reference, operation will delete the document.
    /// </summary>
    public Document.Builder? PatchDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> to delete the document.
    /// </summary>
    public Document.Builder? DeleteDocument { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentTransform"/> of the document node to transform.
    /// </summary>
    public DocumentTransform.Builder? TransformDocument { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="TransactionResponse{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="PatchDocument"/>,
    /// <see cref="DeleteDocument"/> and
    /// <see cref="TransformDocument"/> are a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<WriteDocumentRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (PatchDocument == null &&
            DeleteDocument == null &&
            TransformDocument == null)
        {
            throw new ArgumentException($"All " +
                $"{nameof(PatchDocument)}, " +
                $"{nameof(DeleteDocument)} and " +
                $"{nameof(TransformDocument)} are a null reference. Provide at least one argument.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("writes");
        writer.WriteStartArray();
        if (PatchDocument != null)
        {
            foreach (var document in PatchDocument.Documents)
            {
                if (document.GetModel() is object obj)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WritePropertyName("fields");
                    ModelHelpers.BuildUtf8JsonWriter(Config, writer, obj.GetType(), obj, document, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                    WriteDeleteDocument(writer, document.Reference);
                }
            }
        }
        if (DeleteDocument != null)
        {
            foreach (var document in DeleteDocument.Documents)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("delete");
                writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                writer.WriteEndObject();
                WriteDeleteDocument(writer, document.Reference);
            }
        }
        if (TransformDocument != null)
        {
            foreach (var documentTransform in TransformDocument.DocumentTransforms)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("transform");
                writer.WriteStartObject();
                writer.WritePropertyName("document");
                writer.WriteStringValue(documentTransform.DocumentReference.BuildUrlCascade(Config.ProjectId));
                writer.WritePropertyName("fieldTransforms");
                writer.WriteStartArray();
                foreach (var fieldTransform in documentTransform.FieldTransform.FieldTransforms)
                {
                    var documentFieldPath = ClassMemberHelpers.GetDocumentFieldPath(fieldTransform.ModelType, fieldTransform.PropertyNamePath, jsonSerializerOptions);
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
                                ModelHelpers.BuildUtf8JsonWriterObject(Config, writer, obj?.GetType(), obj, jsonSerializerOptions, null, null);
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
                                ModelHelpers.BuildUtf8JsonWriterObject(Config, writer, obj?.GetType(), obj, jsonSerializerOptions, null, null);
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
                            writer.WriteStringValue(setToServerValueTransform.SetToServerValue.ToEnumString());
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
        if (Transaction != null)
        {
            writer.WritePropertyName("transaction");
            writer.WriteStringValue(Transaction.Token);
        }
        writer.WriteEndObject();

        await writer.FlushAsync();

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
        if (executeResult == null)
        {
            return new(this, executeException);
        }

        return new(this, null);
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":commit")}";
    }

    private void WriteDeleteDocument(Utf8JsonWriter writer, DocumentReference documentReference)
    {
        ArgumentNullException.ThrowIfNull(Config);

        writer.WriteStartObject();
        writer.WritePropertyName("delete");
        writer.WriteStringValue(documentReference.BuildUrlCascade(Config.ProjectId));
        writer.WriteEndObject();
    }
}
