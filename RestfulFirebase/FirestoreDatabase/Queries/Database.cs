
using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The database instance of the firestore.
/// </summary>
public class Database
{
    #region Properties

    /// <summary>
    /// Gets the database id of the firestore database.
    /// </summary>
    public string DatabaseId { get; }

    #endregion

    #region Initializers

    private Database(string? databaseId)
    {
        if (databaseId == null || string.IsNullOrEmpty(databaseId))
        {
            databaseId = "(default)";
        }

        DatabaseId = databaseId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an instance of <see cref="Database"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="Database"/>.
    /// </returns>
    public static Database Query(string? databaseId = default)
    {
        return new Database(databaseId);
    }

    /// <summary>
    /// Creates a root collection reference <see cref="CollectionReference"/>.
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

        return new CollectionReference(this, null, collectionId);
    }

    /// <summary>
    /// Creates a multiple document reference <see cref="Queries.MultipleDocumentReferences"/>.
    /// </summary>
    /// <param name="documentReferences">
    /// The list of document references.
    /// </param>
    /// <returns>
    /// The created <see cref="Queries.MultipleDocumentReferences"/>.
    /// </returns>
    public MultipleDocumentReferences MultipleDocumentReferences(List<DocumentReference>? documentReferences = null)
    {
        return new MultipleDocumentReferences(this, null, documentReferences);
    }

    /// <summary>
    /// Creates a multiple documents <see cref="Queries.MultipleDocuments{T}"/>.
    /// </summary>
    /// <param name="partialDocuments">
    /// The list of partial documents.
    /// </param>
    /// <param name="documents">
    /// The list of documents.
    /// </param>
    /// <returns>
    /// The created <see cref="Queries.MultipleDocuments{T}"/>.
    /// </returns>
    public MultipleDocuments<T> MultipleDocuments<T>(List<PartialDocument<T>>? partialDocuments = null, List<Document<T>>? documents = null)
        where T : class
    {
        return new MultipleDocuments<T>(this, null, partialDocuments, documents);
    }

    #endregion
}
