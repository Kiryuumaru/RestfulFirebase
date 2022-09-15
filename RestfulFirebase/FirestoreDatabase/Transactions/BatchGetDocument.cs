using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Query;
using RestfulFirebase.FirestoreDatabase;
using System.Transactions;
using RestfulFirebase.Common.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to get the multiple <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class BatchGetDocumentRequest<T> : FirestoreDatabaseRequest<BatchGetDocumentResponse<T>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the existing <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public IEnumerable<Document<T>>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node.
    /// </summary>
    public MultipleDocumentReference? Reference
    {
        get => Query as MultipleDocumentReference;
        set => Query = value;
    }

    /// <inheritdoc cref="BatchGetDocumentRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="BatchGetDocumentResponse{T}"/> with the batch get result <see cref="BatchGetDocuments{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Documents"/> and <see cref="Reference"/> is a null reference.
    /// </exception>
    internal override async Task<BatchGetDocumentResponse<T>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        if (Documents == null && Reference == null)
        {
            throw new ArgumentException($"Both {nameof(Documents)} and {nameof(Reference)} is a null reference. Provide at least one to get.");
        }

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("documents");
            writer.WriteStartArray();
            if (Documents != null)
            {
                foreach (var document in Documents)
                {
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                }
            }
            else if (Reference != null)
            {
                foreach (var reference in Reference.GetDocumentReferences())
                {
                    writer.WriteStringValue(reference.BuildUrlCascade(Config.ProjectId));
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            await writer.FlushAsync();

            var response = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);
            IReadOnlyList<Document<T>> found = new List<Document<T>>();
            IReadOnlyList<DocumentReference> missing = new List<DocumentReference>();
            DateTimeOffset readTime = default;

            return new BatchGetDocumentResponse<T>(this, new BatchGetDocuments<T>(found, missing, readTime), null);
        }
        catch (Exception ex)
        {
            return new BatchGetDocumentResponse<T>(this, null, ex);
        }
    }

    internal override string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return GetQuery().BuildUrl(Config.ProjectId, ":batchGet");
    }
}

/// <summary>
/// The response of the <see cref="BatchGetDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class BatchGetDocumentResponse<T> : TransactionResponse<BatchGetDocumentRequest<T>, BatchGetDocuments<T>>
    where T : class
{
    internal BatchGetDocumentResponse(BatchGetDocumentRequest<T> request, BatchGetDocuments<T>? response, Exception? error)
        : base(request, response, error)
    {

    }
}

/// <summary>
/// The result of the <see cref="BatchGetDocumentRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class BatchGetDocuments<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<Document<T>> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReference> Missing { get; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> time at which the document was read.
    /// </summary>
    public DateTimeOffset ReadTime { get; }

    internal BatchGetDocuments(IReadOnlyList<Document<T>> found, IReadOnlyList<DocumentReference> missing, DateTimeOffset readTime)
    {
        Found = found;
        Missing = missing;
        ReadTime = readTime;
    }
}
