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
using RestfulFirebase.FirestoreDatabase.Writes;

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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document>> CreateDocument(object model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        => App.FirestoreDatabase.ExecuteCreate(model, this, documentId, authorization, cancellationToken);

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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<Document<T>>> CreateDocument<T>(T model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class => App.FirestoreDatabase.ExecuteCreate(model, this, documentId, authorization, cancellationToken);

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <param name="documentNames">
    /// The name of the documents to get.
    /// </param>
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
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
    public async Task<HttpResponse<GetDocumentsResult>> GetDocuments(IEnumerable<string> documentNames, IEnumerable<Document>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentNames.Select(i => Document(i)), null, cacheDocuments, transaction, authorization, cancellationToken);
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
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
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
    public async Task<HttpResponse<GetDocumentsResult<T>>> GetDocuments<T>(IEnumerable<string> documentNames, IEnumerable<Document>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult<T>> response = new();

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentNames.Select(i => Document(i)), null, cacheDocuments, transaction, authorization, cancellationToken);
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> PatchDocuments<T>(IEnumerable<(string documentName, T? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        return App.FirestoreDatabase.Write()
            .Patch(documents.Select(i => new Document<T>(Document(i.documentName), i.model)))
            .Run(transaction, authorization, cancellationToken);
    }

    /// <summary>
    /// Request to perform a patch and get operation to documents.
    /// </summary>
    /// <param name="documents">
    /// The requested document to patch.
    /// </param>
    /// <param name="cacheDocuments">
    /// The cache of documents to recycle if it matched its reference.
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse<GetDocumentsResult<TModel>>> PatchAndGetDocuments<TModel>(IEnumerable<(string documentName, TModel? model)> documents, IEnumerable<Document>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        return App.FirestoreDatabase.Write()
            .Patch(documents.Select(i => new Document<TModel>(Document(i.documentName), i.model)))
            .RunAndGet<TModel>(cacheDocuments, transaction, authorization, cancellationToken);
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Task<HttpResponse> DeleteDocuments(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        return App.FirestoreDatabase.Write()
            .Delete(documentNames.Select(i => Document(i)))
            .Run(transaction, authorization, cancellationToken);
    }

    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <param name="documentName">
    /// The requested document names to transform.
    /// </param>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentName"/> is a <c>null</c> reference.
    /// </exception>
    public WriteWithDocumentTransform Transform(string documentName)
    {
        return App.FirestoreDatabase.Write().Transform(Document(documentName));
    }

    /// <summary>
    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
    /// </summary>
    /// <param name="documentName">
    /// The requested document names to transform.
    /// </param>
    /// <typeparam name="TModel">
    /// The type of the model of the document to transform.
    /// </typeparam>
    /// <returns>
    /// The write with new added <see cref="DocumentTransform"/> to transform.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentName"/> is a <c>null</c> reference.
    /// </exception>
    public WriteWithDocumentTransform<TModel> Transform<TModel>(string documentName)
        where TModel : class
    {
        return App.FirestoreDatabase.Write().Transform<TModel>(Document(documentName));
    }

    /// <summary>
    /// Creates a structured <see cref="QueryRoot"/>.
    /// </summary>
    /// <param name="allDescendants">
    /// If specified <c>true</c>, the query will select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created structured <see cref="QueryRoot"/>
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public QueryRoot Query(bool allDescendants = false)
    {
        if (allDescendants && Parent != null)
        {
            ArgumentException.Throw($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        QueryRoot query = new(App, null, Parent);

        query.From(allDescendants, Id);

        return query;
    }

    /// <summary>
    /// Creates a structured <see cref="QueryRoot{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="allDescendants">
    /// If specified <c>true</c>, the query will select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created structured <see cref="QueryRoot{TModel}"/>
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public QueryRoot<TModel> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(bool allDescendants = false)
        where TModel : class
    {
        if (allDescendants && Parent != null)
        {
            ArgumentException.Throw($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        QueryRoot<TModel> query = new(App, Parent);

        query.From(allDescendants, Id);

        return query;
    }
}
