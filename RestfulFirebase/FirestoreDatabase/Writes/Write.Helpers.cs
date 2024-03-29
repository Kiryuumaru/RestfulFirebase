﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulHelpers.Common;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse> ExecuteCommit(Write write, CancellationToken cancellationToken)
    {
        string url =
            $"{FirestoreDatabaseApi.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(FirestoreDatabaseApi.FirestoreDatabaseDocumentsEndpoint, App.Config.ProjectId, ":commit")}";

        JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("writes");
        writer.WriteStartArray();
        foreach (var document in write.PatchDocuments)
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
        foreach (var deleteDocumentReference in write.DeleteDocuments)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("delete");
            writer.WriteStringValue(deleteDocumentReference.BuildUrlCascade(App.Config.ProjectId));
            writer.WriteEndObject();
        }
        foreach (var documentTransform in write.TransformDocuments)
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
                string? fieldPath = null;
                Type? fieldType = null;
                if (fieldTransform.NamePath.Length == 1 && fieldTransform.NamePath[0] == DocumentFieldHelpers.DocumentName)
                {
                    fieldPath = DocumentFieldHelpers.DocumentName;
                }
                else if (!fieldTransform.IsNamePathAPropertyPath)
                {
                    fieldPath = string.Join(".", fieldTransform.NamePath);
                }
                else if (documentTransform.ModelType != null)
                {
                    var documentFieldPath = ModelFieldHelpers.GetModelFieldPath(documentTransform.ModelType, fieldTransform.NamePath, jsonSerializerOptions);
                    fieldPath = string.Join(".", documentFieldPath.Select(i => i.ModelFieldName));
                    fieldType = documentFieldPath.LastOrDefault()?.Type;
                }
                else
                {
                    ArgumentException.Throw($"OrderBy query with property path enabled requires a query with types");
                }

                switch (fieldTransform)
                {
                    case AppendMissingElementsTransform appendMissingElementsTransform:

                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldPath);
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

                        if (fieldType != null && incrementParamNumberType == NumberType.Double)
                        {
                            NumberType propertyNumberType = NumberTypeHelpers.GetNumberType(fieldType);

                            if (propertyNumberType != NumberType.Double)
                            {
                                ArgumentException.Throw($"Increment type mismatch. \"{fieldType}\" cannot increment with \"{incrementValueType}\"");
                            }
                        }

                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldPath);
                        writer.WritePropertyName("increment");
                        writer.WriteStartObject();
                        if (incrementParamNumberType == NumberType.Integer)
                        {
                            writer.WritePropertyName("integerValue");
                        }
                        else if (incrementParamNumberType == NumberType.Double)
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

                        if (fieldType != null && maximumParamNumberType == NumberType.Double)
                        {
                            NumberType propertyNumberType = NumberTypeHelpers.GetNumberType(fieldType);

                            if (propertyNumberType != NumberType.Double)
                            {
                                ArgumentException.Throw($"Increment type mismatch. \"{fieldType}\" cannot maximum with \"{maximumParamNumberType}\"");
                            }
                        }

                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldPath);
                        writer.WritePropertyName("maximum");
                        writer.WriteStartObject();
                        if (maximumParamNumberType == NumberType.Integer)
                        {
                            writer.WritePropertyName("integerValue");
                        }
                        else if (maximumParamNumberType == NumberType.Double)
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

                        if (fieldType != null && minimumParamNumberType == NumberType.Double)
                        {
                            NumberType propertyNumberType = NumberTypeHelpers.GetNumberType(fieldType);

                            if (propertyNumberType != NumberType.Double)
                            {
                                ArgumentException.Throw($"Increment type mismatch. \"{fieldType}\" cannot minimum with \"{minimumParamNumberType}\"");
                            }
                        }

                        writer.WriteStartObject();
                        writer.WritePropertyName("fieldPath");
                        writer.WriteStringValue(fieldPath);
                        writer.WritePropertyName("minimum");
                        writer.WriteStartObject();
                        if (minimumParamNumberType == NumberType.Integer)
                        {
                            writer.WritePropertyName("integerValue");
                        }
                        else if (minimumParamNumberType == NumberType.Double)
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
                        writer.WriteStringValue(fieldPath);
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
                        writer.WriteStringValue(fieldPath);
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
        writer.WriteEndArray();
        FirestoreDatabaseApi.BuildTransaction(writer, write.TransactionUsed, false);
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        return await App.FirestoreDatabase.ExecutePost(write.AuthorizationUsed, stream, url, cancellationToken);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<GetDocumentsResult>> ExecuteCreate(Write write, CancellationToken cancellationToken)
    {
        HttpResponse response = new();
        List<DocumentTimestamp> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        List<Task> tasks = new();
        foreach (var (model, collectionReference, documentName) in write.CreateDocuments)
        {
            tasks.Add(Task.Run(async delegate
            {
                Type modelType = model.GetType();

                if (!modelType.IsClass)
                {
                    ArgumentException.Throw($"\"{nameof(model)}\" is not a class type. Document models should be a class type.");
                }

                JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

                var (jsonDocument, createDocumentResponse) = await ExecuteCreate(modelType, model, null, collectionReference, documentName, write.AuthorizationUsed, jsonSerializerOptions, cancellationToken);
                response.Append(createDocumentResponse);
                if (!createDocumentResponse.IsError &&
                    jsonDocument != null &&
                    ModelBuilderHelpers.Parse(
                        App,
                        null,
                        modelType,
                        model,
                        documentName == null ? null : write.CacheDocuments.FirstOrDefault(i => i.Reference.Id == documentName),
                        jsonDocument.RootElement.EnumerateObject(),
                        jsonSerializerOptions) is Document document)
                {
                    found.Add(new(document, DateTimeOffset.UtcNow, false));
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        return new(new(found.AsReadOnly(), missing.AsReadOnly()), response);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<GetDocumentsResult<TModel>>> ExecuteCreate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(Write write, CancellationToken cancellationToken)
        where TModel : class
    {
        HttpResponse response = new();
        List<DocumentTimestamp<TModel>> found = new();
        List<DocumentReferenceTimestamp> missing = new();

        List<Task> tasks = new();
        foreach (var (model, collectionReference, documentName) in write.CreateDocuments)
        {
            tasks.Add(Task.Run(async delegate
            {
                Type modelType = model.GetType();

                if (!modelType.IsClass)
                {
                    ArgumentException.Throw($"\"{nameof(model)}\" is not a class type. Document models should be a class type.");
                }

                JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

                var (jsonDocument, createDocumentResponse) = await ExecuteCreate(modelType, model, null, collectionReference, documentName, write.AuthorizationUsed, jsonSerializerOptions, cancellationToken);
                response.Append(createDocumentResponse);
                if (!createDocumentResponse.IsError &&
                    jsonDocument != null &&
                    ModelBuilderHelpers.Parse<TModel>(
                        App,
                        null,
                        model,
                        documentName == null ? null : write.CacheDocuments.FirstOrDefault(i => i.Reference.Id == documentName),
                        jsonDocument.RootElement.EnumerateObject(),
                        jsonSerializerOptions) is Document<TModel> document)
                {
                    found.Add(new(document, DateTimeOffset.UtcNow, false));
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        return new(new(found.AsReadOnly(), missing.AsReadOnly()), response);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteCreate(
        Type? objType,
        object? obj,
        Document? document,
        CollectionReference collectionReference,
        string? documentId,
        IAuthorization? authorization,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken)
    {
        QueryBuilder qb = new();
        if (documentId != null)
        {
            qb.Add("documentId", documentId);
        }
        else if (document?.Reference.Id != null)
        {
            qb.Add("documentId", document.Reference.Id);
        }
        string url = collectionReference.BuildUrl(App.Config.ProjectId, qb.Build());

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("fields");
        if (objType != null)
        {
            ModelBuilderHelpers.BuildUtf8JsonWriter(App.Config, writer, objType, obj, document, jsonSerializerOptions);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);

        var response = await App.FirestoreDatabase.ExecutePost(authorization, stream, url, cancellationToken);
        if (response.IsError || response.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return (null, response);
        }

#if NET6_0_OR_GREATER
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream? contentStream = lastHttpTransaction.ResponseMessage == null ? null : await lastHttpTransaction.ResponseMessage.Content.ReadAsStreamAsync();
#endif

        return contentStream == null ? (null, response) : (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }
}
