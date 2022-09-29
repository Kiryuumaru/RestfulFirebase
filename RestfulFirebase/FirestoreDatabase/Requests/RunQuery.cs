using System;
using RestfulFirebase.Common.Requests;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.Common.Utilities;
using System.Reflection;
using RestfulFirebase.Common.Attributes;
using System.Collections.Generic;
using RestfulFirebase.FirestoreDatabase.References;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Requests;

/// <summary>
/// Request to run a query.
/// </summary>
public class RunQueryRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : FirestoreDatabaseRequest<TransactionResponse<RunQueryRequest<T>, RunQueryResult<T>>>
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="System.Text.Json.JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the cache <see cref="Document{T}"/> documents to get and patch.
    /// </summary>
    public Document<T>.Builder? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="References.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the order to sort results by.
    /// </summary>
    public FromQuery.Builder? From { get; set; }

    /// <summary>
    /// Gets or sets the order to sort results by.
    /// </summary>
    public OrderByQuery.Builder? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the number of results to skip. Applies before limit, but after all other constraints. Must be >= 0 if specified.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return. Applies after all other constraints. Must be >= 0 if specified.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Transactions.Transaction"/> for atomic operation.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <inheritdoc cref="RunQueryRequest{T}"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/> with the result <see cref="Transaction"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="From"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    internal override async Task<TransactionResponse<RunQueryRequest<T>, RunQueryResult<T>>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(From);
        if (From.FromQuery.FirstOrDefault() is not FromQuery firstFromQuery)
        {
            throw new ArgumentException($"\"{nameof(From)}\" must contain at least one parameter.");
        }

        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption(JsonSerializerOptions);

        string url;
        if (DocumentReference != null)
        {
            if (From.FromQuery.Any(i => i.CollectionReference.Parent != null && i.CollectionReference.Parent != DocumentReference))
            {
                throw new ArgumentException($"\"{nameof(DocumentReference)}\" is provided but one or more \"{nameof(From)}\" has different parent document.");
            }

            url = DocumentReference.BuildUrl(Config.ProjectId, ":runQuery");
        }
        else
        {
            if (From.FromQuery.Count == 1 &&
                firstFromQuery.CollectionReference.Parent != null)
            {
                url = firstFromQuery.CollectionReference.Parent.BuildUrl(Config.ProjectId, ":runQuery");
            }
            else
            {
                if (From.FromQuery.Any(i => i.CollectionReference.Parent != firstFromQuery.CollectionReference.Parent))
                {
                    throw new ArgumentException($"\"{nameof(From)}\" has different parent document.");
                }

                url =
                    $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/" +
                    $"{string.Format(Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint, Config.ProjectId, ":runQuery")}";
            }
        }

        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("structuredQuery");
        writer.WriteStartObject();
        writer.WritePropertyName("from");
        writer.WriteStartArray();
        foreach (var from in From.FromQuery)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("collectionId");
            writer.WriteStringValue(from.CollectionReference.Id);
            writer.WritePropertyName("allDescendants");
            writer.WriteBooleanValue(from.AllDescendants);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        if (OrderBy != null)
        {
            Type objType = typeof(T);

            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            writer.WritePropertyName("orderBy");
            writer.WriteStartArray();
            foreach (var orderBy in OrderBy.OrderByQuery)
            {
                var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, null, orderBy.PropertyName, jsonSerializerOptions);

                if (documentField == null)
                {
                    throw new ArgumentException($"OrderBy property name \"{orderBy.PropertyName}\" does not exist in the model \"{objType.Name}\".");
                }

                writer.WriteStartObject();
                writer.WritePropertyName("field");
                writer.WriteStartObject();
                writer.WritePropertyName("fieldPath");
                writer.WriteStringValue(documentField.DocumentFieldName);
                writer.WriteEndObject();
                writer.WritePropertyName("direction");
                writer.WriteStringValue(orderBy.OrderDirection == Enums.OrderDirection.Ascending ? "ASCENDING" : "DESCENDING");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        if (Offset != null)
        {
            writer.WritePropertyName("offset");
            writer.WriteNumberValue(Offset.Value);
        }
        if (Limit != null)
        {
            writer.WritePropertyName("limit");
            writer.WriteNumberValue(Limit.Value);
        }
        writer.WriteEndObject();
        if (Transaction != null)
        {
            writer.WritePropertyName("transaction");
            writer.WriteStringValue(Transaction.Token);
        }
        writer.WriteEndObject();

        await writer.FlushAsync();

        var (executeResult, executeException) = await ExecuteWithContent(stream, HttpMethod.Post, url);
        if (executeResult == null)
        {
            return new(this, null, executeException);
        }

        using Stream contentStream = await executeResult.Content.ReadAsStreamAsync();
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(contentStream);

        List<DocumentTimestamp<T>> foundDocuments = new();
        int? skippedResults = null;
        DateTimeOffset? skippedReadTime = null;

        foreach (var doc in jsonDocument.RootElement.EnumerateArray())
        {
            if (doc.TryGetProperty("readTime", out JsonElement readTimeProperty) &&
                readTimeProperty.GetDateTimeOffset() is DateTimeOffset readTime)
            {
                DocumentReference? documentReference = null;
                Document<T>? document = null;
                T? model = null;
                if (doc.TryGetProperty("document", out JsonElement foundPropertyDocument))
                {
                    if (foundPropertyDocument.TryGetProperty("name", out JsonElement foundNameProperty) &&
                        DocumentReference.Parse(foundNameProperty, jsonSerializerOptions) is DocumentReference docRef)
                    {
                        documentReference = docRef;

                        if (Document != null &&
                            Document.Documents.FirstOrDefault(i => i.Reference.Equals(docRef)) is Document<T> foundDocument)
                        {
                            document = foundDocument;
                            model = foundDocument.Model;
                        }
                    }

                    if (Document<T>.Parse(documentReference, model, document, foundPropertyDocument.EnumerateObject(), jsonSerializerOptions) is Document<T> found)
                    {
                        foundDocuments.Add(new DocumentTimestamp<T>(found, readTime));
                    }
                }
                else if (doc.TryGetProperty("skippedResults", out JsonElement skippedResultsProperty) &&
                    skippedResultsProperty.TryGetInt32(out int parsedSkippedResults))
                {
                    skippedResults = parsedSkippedResults;
                    skippedReadTime = readTime;
                }
            }
        }

        return new(this, new(foundDocuments, skippedResults, skippedReadTime), null);
    }
}

/// <summary>
/// The result of the <see cref="RunQueryResult{T}"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class RunQueryResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Found { get; }

    /// <summary>
    /// Gets the number of results that have been skipped due to an offset between the last response and the current response.
    /// </summary>
    public int? SkippedResults { get; }

    /// <summary>
    /// Gets the time at which the skipped document was read.
    /// </summary>
    public DateTimeOffset? SkippedReadTime { get; }

    internal RunQueryResult(IReadOnlyList<DocumentTimestamp<T>> found, int? skippedResults, DateTimeOffset? skippedReadTime)
    {
        Found = found;
        SkippedResults = skippedResults;
        SkippedReadTime = skippedReadTime;
    }
}