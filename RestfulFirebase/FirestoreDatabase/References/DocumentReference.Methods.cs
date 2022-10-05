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
    /// Creates a document <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="model">
    /// The model of the document
    /// </param>
    /// <returns>
    /// The <see cref="Document{T}"/>.
    /// </returns>
    public async Task<Document<T>> CreateDocument<T>(T? model = null, Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        Document<T> doc = new(this, model);

        await App.FirestoreDatabase.PatchDocument(doc, transaction, authorization, jsonSerializerOptions, cancellationToken);

        return doc;
    }

    /// <summary>
    /// Creates a document <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="model">
    /// The model of the document
    /// </param>
    /// <returns>
    /// The <see cref="Document{T}"/>.
    /// </returns>
    public async Task<HttpResponse<DocumentTimestamp<T>>> GetDocument<T>(Transaction? transaction = default, IAuthorization? authorization = default, JsonSerializerOptions? jsonSerializerOptions = default, CancellationToken cancellationToken = default)
        where T : class
    {
        var response = await App.FirestoreDatabase.GetDocument<T>(this, transaction, authorization, jsonSerializerOptions, cancellationToken);
        if (response.IsError)
        {
            return new(response);
        }

        if (response.Result.Found.FirstOrDefault() is DocumentTimestamp<T> documentTimestamp)
        {
            return new(documentTimestamp, response);
        }

        return new(null, response);
    }
}
