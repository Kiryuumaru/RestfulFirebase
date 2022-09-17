using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using System.Transactions;
using RestfulFirebase.Common.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to write the multiple <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class WriteDocumentsRequest<T> : FirestoreDatabaseRequest<TransactionResponse<WriteDocumentsRequest<T>, PatchDocumentsResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="Document{T}"/> documents.
    /// </summary>
    public IEnumerable<Document<T>>? Documents { get; set; }

    /// <inheritdoc cref="WriteDocumentsRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="PatchDocumentsResult{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="Documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<WriteDocumentsRequest<T>, PatchDocumentsResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(Documents);

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        try
        {
            using MemoryStream stream = new();
            Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();
            writer.WritePropertyName("writes");
            writer.WriteStartArray();
            foreach (var document in Documents)
            {
                if (document.Model != null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("update");
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WritePropertyName("fields");
                    PopulateDocument(Config, writer, document.Model, null, jsonSerializerOptions);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("delete");
                    writer.WriteStringValue(document.Reference.BuildUrlCascade(Config.ProjectId));
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            await writer.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            string dasd = reader.ReadToEnd();

            var response = await ExecuteWithContent(stream, HttpMethod.Post, BuildUrl());
            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

            string asda = jsonDocument.RootElement.ToString();

            List<DocumentTimestamp<T>> foundDocuments = new();
            List<DocumentReferenceTimestamp> missingDocuments = new();

            return new(this, new PatchDocumentsResult<T>(foundDocuments.AsReadOnly(), missingDocuments.AsReadOnly()), null);
        }
        catch (Exception ex)
        {
            return new(this, null, ex);
        }
    }

    internal string BuildUrl()
    {
        ArgumentNullException.ThrowIfNull(Config);

        return
            $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
            $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":commit")}";
    }
}

/// <summary>
/// The result of the <see cref="GetDocumentsRequest{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class PatchDocumentsResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal PatchDocumentsResult(IReadOnlyList<DocumentTimestamp<T>> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}
