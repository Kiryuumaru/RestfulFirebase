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
using RestfulFirebase.FirestoreDatabase.Queries;

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

    /// <summary>
    /// Request to create a <see cref="Models.Document"/>.
    /// </summary>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="documentId">
    /// The client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
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
    public Task<HttpResponse<Document>> CreateDocument(object model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        => App.FirestoreDatabase.CreateDocument(model, this, documentId, authorization, cancellationToken);

    /// <summary>
    /// Request to create a <see cref="Models.Document"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="documentId">
    /// The client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
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
    public Task<HttpResponse<Document<T>>> CreateDocument<T>(T model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class => App.FirestoreDatabase.CreateDocument(model, this, documentId, authorization, cancellationToken);

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <param name="documentNames">
    /// The name of the documents to get.
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
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult>> GetDocuments(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentNames.Select(i => Document(i)), Array.Empty<Document>(), transaction, authorization, cancellationToken);
        response.Append(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="documentNames">
    /// The name of the documents to get.
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
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="GetDocumentResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult<T>>> GetDocuments<T>(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult<T>> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentNames.Select(i => Document(i)), Array.Empty<Document<T>>(), transaction, authorization, cancellationToken);
        response.Append(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to perform a patch operation to documents.
    /// </summary>
    /// <param name="documents">
    /// The requested document to patch.
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
    /// <exception cref="ArgumentException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> PatchDocuments<T>(IEnumerable<(string documentName, T? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        return App.FirestoreDatabase.WriteDocument(documents.Select(i => new Document<T>(Document(i.documentName), i.model)), null, null, transaction, authorization, cancellationToken);
    }

    /// <summary>
    /// Request to perform a patch operation to documents.
    /// </summary>
    /// <param name="documents">
    /// The requested document to patch.
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
    /// <exception cref="ArgumentException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult<T>>> PatchAndGetDocuments<T>(IEnumerable<(string documentName, T? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        IEnumerable<Document<T>> docs = documents.Select(i => new Document<T>(Document(i.documentName), i.model));

        HttpResponse<GetDocumentsResult<T>> response = new();

        var patchDocumentResponse = await App.FirestoreDatabase.WriteDocument(docs, null, null, transaction, authorization, cancellationToken);
        response.Append(patchDocumentResponse);
        if (patchDocumentResponse.IsError)
        {
            return response;
        }

        var getDocumentResponse = await App.FirestoreDatabase.GetDocuments(docs, transaction, authorization, cancellationToken);
        response.Append(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to perform a delete operation to documents.
    /// </summary>
    /// <param name="documentNames">
    /// The requested document names to delete.
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
    /// <exception cref="ArgumentException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> DeleteDocuments(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        return App.FirestoreDatabase.WriteDocument(null, documentNames.Select(i => Document(i)), null, transaction, authorization, cancellationToken);
    }

    /// <summary>
    /// Creates a structured <see cref="Queries.Query"/>.
    /// </summary>
    /// <param name="allDescendants">
    /// If specified <c>true</c>, the query will select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created structured <see cref="Queries.Query"/>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public Query Query(bool allDescendants = false)
    {
        if (allDescendants && Parent != null)
        {
            throw new ArgumentException($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        Query query = new(App, null, Parent);

        query.From(allDescendants, Id);

        return query;
    }

    /// <summary>
    /// Creates a structured <see cref="Queries.Query{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="allDescendants">
    /// If specified <c>true</c>, the query will select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created structured <see cref="Queries.Query{TModel}"/>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public Query<TModel> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(bool allDescendants = false)
        where TModel : class
    {
        if (allDescendants && Parent != null)
        {
            throw new ArgumentException($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        Query<TModel> query = new(App, Parent);

        query.From(allDescendants, Id);

        return query;
    }
}
