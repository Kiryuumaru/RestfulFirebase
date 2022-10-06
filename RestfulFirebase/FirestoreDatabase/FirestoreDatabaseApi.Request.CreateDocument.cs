using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Reflection;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<(JsonDocument?, HttpResponse)> ExecuteCreateDocument(
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

        var response = await ExecutePost(authorization, stream, url, cancellationToken);
        if (response.IsError || response.HttpTransactions.LastOrDefault() is not HttpTransaction lastHttpTransaction)
        {
            return (null, response);
        }

#if NET6_0_OR_GREATER
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        using Stream contentStream = await lastHttpTransaction.HttpResponseMessage.Content.ReadAsStreamAsync();
#endif

        return (await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken), response);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<Document>> CreateDocument(object model, CollectionReference collectionReference, string? documentId = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(collectionReference);

        Type modelType = model.GetType();

        if (!modelType.IsClass)
        {
            throw new ArgumentException($"\"{nameof(model)}\" is not a class type. Document models should be a class type.");
        }

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);


        HttpResponse<Document> response = new();

        var (jsonDocument, createDocumentResponse) = await ExecuteCreateDocument(modelType, model, null, collectionReference, documentId, authorization, configuredJsonSerializerOptions, cancellationToken);
        response.Concat(createDocumentResponse);
        if (createDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        return response.Concat(Document.Parse(App, null, modelType, model, null, jsonDocument.RootElement.EnumerateObject(), configuredJsonSerializerOptions));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal async Task<HttpResponse<Document<T>>> CreateDocument<T>(T model, CollectionReference collectionReference, string? documentId = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(collectionReference);

        JsonSerializerOptions configuredJsonSerializerOptions = ConfigureJsonSerializerOption(jsonSerializerOptions);

        Type modelType = typeof(T);

        HttpResponse<Document<T>> response = new();

        var (jsonDocument, createDocumentResponse) = await ExecuteCreateDocument(modelType, model, null, collectionReference, documentId, authorization, configuredJsonSerializerOptions, cancellationToken);
        response.Concat(createDocumentResponse);
        if (createDocumentResponse.IsError || jsonDocument == null)
        {
            return response;
        }

        return response.Concat(Document<T>.Parse(App, null, model, null, jsonDocument.RootElement.EnumerateObject(), configuredJsonSerializerOptions));
    }
}
