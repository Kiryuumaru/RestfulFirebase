﻿using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Writes;
using RestfulHelpers.Common;

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
    /// The Client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="Models.Document"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="model"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<Document>> CreateDocument(object model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        HttpResponse<Document> response = new();

        var writeResponse = await App.FirestoreDatabase.Write()
            .Create(model, this, documentId)
            .Authorization(authorization)
            .RunAndGetSingle(cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        response.Append(writeResponse.Result?.Found?.Document);

        return response;
    }

    /// <summary>
    /// Request to create a <see cref="Models.Document"/>.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="documentId">
    /// The Client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="Models.Document"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="model"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="model"/> is a value type.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<Document<TModel>>> CreateDocument<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(TModel model, string? documentId = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where TModel : class
    {
        HttpResponse<Document<TModel>> response = new();

        var writeResponse = await App.FirestoreDatabase.Write()
            .Create(model, this, documentId)
            .Authorization(authorization)
            .RunAndGetSingle<TModel>(cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        response.Append(writeResponse.Result?.Found?.Document);

        return response;
    }

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
    public async Task<HttpResponse<GetDocumentsResult>> GetDocuments(IEnumerable<string> documentNames, IEnumerable<Document?>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult> response = new();

        var getResponse = await App.FirestoreDatabase.Fetch()
            .DocumentReference(documentNames.Select(i => Document(i)))
            .Cache(cacheDocuments)
            .Transaction(transaction)
            .Authorization(authorization)
            .Run(cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <typeparam name="TModel">
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
    public async Task<HttpResponse<GetDocumentsResult<TModel>>> GetDocuments<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(IEnumerable<string> documentNames, IEnumerable<Document?>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult<TModel>> response = new();

        var getResponse = await App.FirestoreDatabase.Fetch<TModel>()
            .DocumentReference(documentNames.Select(i => Document(i)))
            .Cache(cacheDocuments)
            .Transaction(transaction)
            .Authorization(authorization)
            .Run(cancellationToken);
        response.Append(getResponse);
        if (getResponse.IsError)
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
    public Task<HttpResponse> PatchDocuments<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(IEnumerable<(string documentName, TModel? model)> documents, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        return App.FirestoreDatabase.Write()
            .Patch(documents.Select(i => new Document<TModel>(Document(i.documentName), i.model)))
            .Transaction(transaction)
            .Authorization(authorization)
            .Run(cancellationToken);
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
    public Task<HttpResponse<GetDocumentsResult<TModel>>> PatchAndGetDocuments<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(IEnumerable<(string documentName, TModel? model)> documents, IEnumerable<Document?>? cacheDocuments = null, Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(documents);

        return App.FirestoreDatabase.Write()
            .Patch(documents.Select(i => new Document<TModel>(Document(i.documentName), i.model)))
            .Cache(cacheDocuments)
            .Transaction(transaction)
            .Authorization(authorization)
            .RunAndGet<TModel>(cancellationToken);
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
            .Transaction(transaction)
            .Authorization(authorization)
            .Run(cancellationToken);
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
    public WriteWithDocumentTransform<TModel> Transform<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(string documentName)
        where TModel : class
    {
        return App.FirestoreDatabase.Write().Transform<TModel>(Document(documentName));
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public Query Query(bool allDescendants = false)
    {
        if (allDescendants && Parent != null)
        {
            ArgumentException.Throw($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        Query query = new(App, null, Parent);

        return query.From(allDescendants, Id);
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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="allDescendants"/> is <c>true</c> and query is not in the root query.
    /// </exception>
    public Query<TModel> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>(bool allDescendants = false)
        where TModel : class
    {
        if (allDescendants && Parent != null)
        {
            ArgumentException.Throw($"\"{nameof(allDescendants)}\" is only applicable from root query.");
        }

        Query<TModel> query = new(App, Parent);

        return query.From(allDescendants, Id);
    }
}
