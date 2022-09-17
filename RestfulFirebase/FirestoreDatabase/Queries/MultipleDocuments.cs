using RestfulFirebase.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using System.Text.Json.Serialization;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The reference for multiple model documents.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the documents.
/// </typeparam>
public class MultipleDocuments<T> : Query, IDocumentReference
     where T : class
{
    #region Properties

    /// <summary>
    /// Gets the multiple partial document references.
    /// </summary>
    public List<PartialDocument<T>> PartialDocuments { get; }

    /// <summary>
    /// Gets the multiple document references.
    /// </summary>
    public List<Document<T>> Documents { get; }

    /// <summary>
    /// Gets the origin collection reference.
    /// </summary>
    public CollectionReference? OriginCollectionReference { get; }

    #endregion

    #region Initializers

    internal MultipleDocuments(Database database, CollectionReference? origin, List<PartialDocument<T>>? partialDocuments, List<Document<T>>? documents)
        : base(database)
    {
        PartialDocuments = partialDocuments ?? new();
        Documents = documents ?? new();
        OriginCollectionReference = origin;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is MultipleDocuments<T> documents &&
               base.Equals(obj) &&
               EqualityComparer<List<PartialDocument<T>>>.Default.Equals(PartialDocuments, documents.PartialDocuments) &&
               EqualityComparer<List<Document<T>>>.Default.Equals(Documents, documents.Documents) &&
               EqualityComparer<CollectionReference?>.Default.Equals(OriginCollectionReference, documents.OriginCollectionReference);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = -1755066021;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<List<PartialDocument<T>>>.Default.GetHashCode(PartialDocuments);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Document<T>>>.Default.GetHashCode(Documents);
        hashCode = hashCode * -1521134295 + (OriginCollectionReference == null ? 0 : EqualityComparer<CollectionReference?>.Default.GetHashCode(OriginCollectionReference));
        return hashCode;
    }

    /// <summary>
    /// Adds partial document to the list.
    /// </summary>
    /// <param name="model">
    /// The model of the partial document to add.
    /// </param>
    /// <param name="documentPath">
    /// The path of the document reference to add.
    /// </param>
    /// <returns>
    /// The sample instance of <see cref="MultipleDocuments{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentPath"/> is a <c>null</c> reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentPath"/> is either empty or it leads to a collection reference.
    /// </exception>
    public MultipleDocuments<T> AddDocument(T model, params string[] documentPath)
    {
        ArgumentNullException.ThrowIfNull(documentPath);

        object currentPath = (object?)OriginCollectionReference ?? Database;
        foreach (var path in documentPath)
        {
            if (currentPath is Database database)
            {
                currentPath = database.Collection(path);
            }
            else if (currentPath is CollectionReference colPath)
            {
                currentPath = colPath.Document(path);
            }
            else if (currentPath is DocumentReference docPath)
            {
                currentPath = docPath.Collection(path);
            }
        }

        if (currentPath is DocumentReference documentReference)
        {
            PartialDocuments.Add(new PartialDocument<T>(documentReference, model));
            return this;
        }
        else if (currentPath is Database)
        {
            throw new ArgumentException("The provided path is empty.");
        }
        else
        {
            throw new ArgumentException("The provided path leads to a collection reference.");
        }
    }

    /// <summary>
    /// Adds partial document to the list.
    /// </summary>
    /// <param name="document">
    /// The partial document to add.
    /// </param>
    /// <returns>
    /// The sample instance of <see cref="MultipleDocuments{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="document"/> is a <c>null</c> reference.
    /// </exception>
    public MultipleDocuments<T> AddDocument(PartialDocument<T> document)
    {
        ArgumentNullException.ThrowIfNull(document);

        PartialDocuments.Add(document);

        return this;
    }

    /// <summary>
    /// Adds document to the list.
    /// </summary>
    /// <param name="document">
    /// The document to add.
    /// </param>
    /// <returns>
    /// The sample instance of <see cref="MultipleDocuments{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="document"/> is a <c>null</c> reference.
    /// </exception>
    public MultipleDocuments<T> AddDocument(Document<T> document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Documents.Add(document);

        return this;
    }

    internal override string[] BuildUrls(string projectId, string? postSegment = null)
    {
        List<string> urls = new();
        urls.AddRange(PartialDocuments.Select(i => i.Reference.BuildUrl(projectId, postSegment)));
        urls.AddRange(Documents.Select(i => i.Reference.BuildUrl(projectId, postSegment)));
        return urls.ToArray();
    }

    internal override string BuildUrlCascade(string projectId)
    {
        return string.Format(
            Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint,
            projectId,
            Database.DatabaseId,
            "");
    }

    #endregion
}
