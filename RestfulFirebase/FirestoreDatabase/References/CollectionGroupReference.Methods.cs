using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class CollectionGroupReference : Reference
{
    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionGroupReference reference &&
               EqualityComparer<CollectionReference[]>.Default.Equals(CollectionReferences, reference.CollectionReferences) &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + EqualityComparer<CollectionReference[]>.Default.GetHashCode(CollectionReferences);
        hashCode = hashCode * -1521134295 + (Parent == null ? 0 : EqualityComparer<DocumentReference?>.Default.GetHashCode(Parent));
        return hashCode;
    }

    /// <summary>
    /// Request to get the documents.
    /// </summary>
    /// <param name="documentNames">
    /// The name of the documents to get.
    /// </param>
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult>> GetDocuments(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult> response = new();

        List<DocumentReference> documentReferences = new();
        foreach (var collectionReference in CollectionReferences)
        {
            documentReferences.AddRange(documentNames.Select(i => collectionReference.Document(i)));
        }

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentReferences, Array.Empty<Document>(), transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(getDocumentResponse);
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentNames"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse<GetDocumentsResult<T>>> GetDocuments<T>(IEnumerable<string> documentNames, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(documentNames);

        HttpResponse<GetDocumentsResult<T>> response = new();

        List<DocumentReference> documentReferences = new();
        foreach (var collectionReference in CollectionReferences)
        {
            documentReferences.AddRange(documentNames.Select(i => collectionReference.Document(i)));
        }

        var getDocumentResponse = await App.FirestoreDatabase.GetDocument(documentReferences, Array.Empty<Document<T>>(), transaction, authorization, jsonSerializerOptions, cancellationToken);
        response.Concat(getDocumentResponse);
        if (getDocumentResponse.IsError)
        {
            return response;
        }

        return response;
    }
}
