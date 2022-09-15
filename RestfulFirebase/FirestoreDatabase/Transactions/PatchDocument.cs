﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using RestfulFirebase.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class PatchDocumentRequest<T> : FirestoreDatabaseRequest<PatchDocumentResponse<T>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the existing <typeparamref name="T"/> model to populate the document fields.
    /// </summary>
    public T? Model { get; set; }

    /// <summary>
    /// Gets or sets the existing <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public Document<T>? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? Reference
    {
        get => Query as DocumentReference;
        set => Query = value;
    }

    /// <inheritdoc cref="PatchDocumentResponse{T}"/>
    /// <returns>
    /// The <see cref="PatchDocumentResponse{T}"/> response of the request.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Reference"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Document"/> and <see cref="Model"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<PatchDocumentResponse<T>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Reference);
        if (Document == null && Model == null)
        {
            throw new ArgumentException($"Both {nameof(Document)} and {nameof(Model)} is a null reference. Provide at least one to patch.");
        }

        try
        {
            JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            PopulateDocument(Config, writer, Model, Document, jsonSerializerOptions);
            writer.WriteEndObject();

            await writer.FlushAsync();

            var response = await ExecuteWithContent(stream, new HttpMethod("PATCH"), BuildUrl());
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            var parsedDocument = ParseDocument(Reference, Model, Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

            return new PatchDocumentResponse<T>(this, parsedDocument, null);

        }
        catch (Exception ex)
        {
            return new PatchDocumentResponse<T>(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="PatchDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class PatchDocumentResponse<T> : TransactionResponse<PatchDocumentRequest<T>, Document<T>>
    where T : class
{
    internal PatchDocumentResponse(PatchDocumentRequest<T> request, Document<T>? response, Exception? error)
        : base(request, response, error)
    {

    }
}
