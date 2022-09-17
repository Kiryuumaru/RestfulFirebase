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

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The multiple reference for documents.
/// </summary>
public class MultipleDocumentReferences : Query, IDocumentReference
{
    #region Properties

    /// <summary>
    /// Gets the multiple document references.
    /// </summary>
    public List<DocumentReference> DocumentReferences { get; }

    /// <summary>
    /// Gets the origin collection reference.
    /// </summary>
    public CollectionReference? OriginCollectionReference { get; }

    #endregion

    #region Initializers

    internal MultipleDocumentReferences(Database database, CollectionReference? origin, List<DocumentReference>? documentReferences)
        : base(database)
    {
        DocumentReferences = documentReferences ?? new();
        OriginCollectionReference = origin;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds document reference to the list.
    /// </summary>
    /// <param name="documentPath">
    /// The path of the document reference to add.
    /// </param>
    /// <returns>
    /// The same instance of <see cref="MultipleDocumentReferences"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentPath"/> is a <c>null</c> reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentPath"/> is either empty or it leads to a collection reference.
    /// </exception>
    public MultipleDocumentReferences AddDocument(params string[] documentPath)
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
            DocumentReferences.Add(documentReference);
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
    /// Adds document reference to the list.
    /// </summary>
    /// <param name="documentReference">
    /// The document reference to add.
    /// </param>
    /// <returns>
    /// The same instance of <see cref="MultipleDocumentReferences"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReference"/> is a <c>null</c> reference.
    /// </exception>
    public MultipleDocumentReferences AddDocument(DocumentReference documentReference)
    {
        ArgumentNullException.ThrowIfNull(documentReference);

        DocumentReferences.Add(documentReference);

        return this;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is MultipleDocumentReferences reference &&
               base.Equals(obj) &&
               EqualityComparer<Database>.Default.Equals(Database, reference.Database) &&
               EqualityComparer<IEnumerable<DocumentReference>>.Default.Equals(DocumentReferences, reference.DocumentReferences);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1943541580;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Database>.Default.GetHashCode(Database);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<DocumentReference>>.Default.GetHashCode(DocumentReferences);
        return hashCode;
    }

    internal override string[] BuildUrls(string projectId, string? postSegment = null)
    {
        return DocumentReferences.Select(i => i.BuildUrl(projectId, postSegment)).ToArray();
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
