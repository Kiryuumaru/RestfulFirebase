using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to get the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class GetDocumentRequest<T> : FirestoreDatabaseRequest<GetDocumentResponse<T>>
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

    /// <inheritdoc cref="GetDocumentResponse{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="GetDocumentResponse{T}"/> with the get result <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Reference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<GetDocumentResponse<T>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Reference);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            var response = await Execute(HttpMethod.Get, BuildUrl());
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            var parsedDocument = ParseDocument(Reference, Model, Document, jsonDocument.RootElement.EnumerateObject(), jsonSerializerOptions);

            return new GetDocumentResponse<T>(this, parsedDocument, null);
        }
        catch (Exception ex)
        {
            return new GetDocumentResponse<T>(this, null, ex);
        }
    }
}

/// <summary>
/// The response of the <see cref="GetDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class GetDocumentResponse<T> : TransactionResponse<GetDocumentRequest<T>, Document<T>>
    where T : class
{
    internal GetDocumentResponse(GetDocumentRequest<T> request, Document<T>? response, Exception? error)
        : base(request, response, error)
    {

    }
}
