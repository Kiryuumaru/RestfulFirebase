using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Common.Http;
using System;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class CollectionReference : Reference
{
    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionReference reference &&
               Id == reference.Id &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
        hashCode = hashCode * -1521134295 + (Parent == null ? 0 : EqualityComparer<DocumentReference?>.Default.GetHashCode(Parent));
        return hashCode;
    }

    /// <summary>
    /// Creates a document reference <see cref="DocumentReference"/>.
    /// </summary>
    /// <param name="id">
    /// The ID of the document reference.
    /// </param>
    /// <returns>
    /// The <see cref="DocumentReference"/> of the specified <paramref name="id"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="id"/> is a <c>null</c> reference.
    /// </exception>
    public DocumentReference Document(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return new DocumentReference(App, id, this);
    }

    /// <inheritdoc cref="FirestoreDatabaseApi.CreateDocument(object, CollectionReference, string?, IAuthorization?, JsonSerializerOptions?, CancellationToken)"/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document>> CreateDocument(object model, string? documentId = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        => App.FirestoreDatabase.CreateDocument(model, this, documentId, authorization, jsonSerializerOptions, cancellationToken);

    /// <inheritdoc cref="FirestoreDatabaseApi.CreateDocument{T}(T, CollectionReference, string?, IAuthorization?, JsonSerializerOptions?, CancellationToken)"/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document<T>>> CreateDocument<T>(T model, string? documentId = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class => App.FirestoreDatabase.CreateDocument(model, this, documentId, authorization, jsonSerializerOptions, cancellationToken);

    public async Task<HttpResponse<Document<T>[]>> PatchDocument<T>(IEnumerable<(string documentName, T? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        List<Document> docs = new();

        foreach (var (documentName, model) in documents)
        {
            docs.Add(new Document<T>(Document(documentName), model));
        }

        HttpResponse<Document<T>[]> response = new();

        var patchResponse = await App.FirestoreDatabase.PatchDocument(docs, transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(patchResponse);
        if (patchResponse.IsError)
        {
            return response;
        }

        return response.Concat(docs.Cast<Document<T>>().ToArray());
    }

    public async Task<HttpResponse<DocumentTimestamp<T>[]>> PatchAndGetDocument<T>(IEnumerable<(string documentName, T? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        List<Document<T>> docs = new();

        foreach (var (documentName, model) in documents)
        {
            docs.Add(new Document<T>(Document(documentName), model));
        }

        HttpResponse<DocumentTimestamp<T>[]> response = new();

        var patchResponse = await App.FirestoreDatabase.PatchDocument(docs.Select(i => (Document)i).ToList(), transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(response);
        if (patchResponse.IsError)
        {
            return response;
        }

        var getResponse = await App.FirestoreDatabase.GetDocument<T>(docs, transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(getResponse);
        if (getResponse.IsError)
        {
            return response;
        }

        return response.Concat(getResponse.Result.Found.ToArray());
    }
}
