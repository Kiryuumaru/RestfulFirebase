using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class DocumentReference : Reference
{
    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is DocumentReference reference &&
               Id == reference.Id &&
               EqualityComparer<CollectionReference>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1057591069;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
        hashCode = hashCode * -1521134295 + EqualityComparer<CollectionReference>.Default.GetHashCode(Parent);
        return hashCode;
    }

    /// <summary>
    /// Creates a collection reference <see cref="CollectionReference"/>.
    /// </summary>
    /// <param name="collectionId">
    /// The ID of the collection reference.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionId"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionId"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionReference Collection(string collectionId)
    {
        ArgumentNullException.ThrowIfNull(collectionId);

        return new CollectionReference(App, collectionId, this);
    }

    /// <summary>
    /// Creates a collection group reference <see cref="CollectionGroupReference"/>.
    /// </summary>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionIds"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionGroupReference CollectionGroup(params string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        CollectionGroupReference reference = new(App, this);

        reference.AddCollection(collectionIds);

        return reference;
    }

    /// <summary>
    /// Creates a collection group reference <see cref="CollectionGroupReference"/>.
    /// </summary>
    /// <param name="allDescendants">
    /// When <c>false</c>, selects only collections that are immediate children of the parent specified in the containing RunQueryRequest. When <c>true</c>, selects all descendant collections.
    /// </param>
    /// <param name="collectionIds">
    /// The ID of the collection references.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionIds"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionGroupReference CollectionGroup(bool allDescendants, params string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        CollectionGroupReference reference = new(App, this);

        reference.AddCollection(allDescendants, collectionIds);

        return reference;
    }

    /// <summary>
    /// Request to create a <see cref="Document"/>.
    /// </summary>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="Document"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="model"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document>> CreateDocument(object model, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        => App.FirestoreDatabase.CreateDocument(model, Parent, Id, authorization, jsonSerializerOptions, cancellationToken);

    /// <summary>
    /// Request to create a <see cref="Models.Document"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="Document"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="model"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document<T>>> CreateDocument<T>(T model, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class => App.FirestoreDatabase.CreateDocument(model, Parent, Id, authorization, jsonSerializerOptions, cancellationToken);

    /// <summary>
    /// Request to get the document.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentResult>> GetDocument(Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        HttpResponse<GetDocumentResult> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(new DocumentReference[] { this }, Array.Empty<Document>(), transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to get the document.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentResult<T>>> GetDocument<T>(Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        HttpResponse<GetDocumentResult<T>> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(new DocumentReference[] { this }, Array.Empty<Document<T>>(), transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to list the <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="pageSize">
    /// The requested page size of the pager <see cref="ListCollectionResult.GetAsyncEnumerator(CancellationToken)"/>.
    /// </param>
    /// <param name="jsonSerializerOptions">
    /// The <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="ListCollectionResult"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<ListCollectionResult>> ListCollection(int? pageSize = null, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.ListCollection(pageSize, this, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a patch operation to document.
    /// </summary>
    /// <param name="model">
    /// The model to patch the document fields. If it is a null reference, operation will delete the document.
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> PatchDocument<T>(T? model, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        return App.FirestoreDatabase.WriteDocument(new Document[] { new Document<T>(this, model) }, null, null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Request to perform a delete operation to a document.
    /// </summary>
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> DeleteDocument(Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.WriteDocument(null, new DocumentReference[] { this }, null, transaction, authorization, jsonSerializerOptions, cancellationToken);
    }
}
