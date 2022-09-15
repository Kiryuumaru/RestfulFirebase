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
using RestfulFirebase.FirestoreDatabase.Abstraction;
using System.Text.Json.Serialization;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Query;

/// <summary>
/// The reference for documents.
/// </summary>
public class MultipleDocumentReference : Query, IDocumentReference
{
    #region Properties

    /// <summary>
    /// Gets the ID of the collection reference.
    /// </summary>
    public IEnumerable<string> Ids { get; }

    /// <summary>
    /// Gets the parent of the document reference.
    /// </summary>
    public CollectionReference Parent { get; }

    #endregion

    #region Initializers

    internal MultipleDocumentReference(Database database, CollectionReference parent, IEnumerable<string> documentIds)
        : base(database)
    {
        Ids = documentIds;
        Parent = parent;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets all the <see cref="DocumentReference"/>.
    /// </summary>
    /// <returns>
    /// All the <see cref="DocumentReference"/>.
    /// </returns>
    public IReadOnlyList<DocumentReference> GetDocumentReferences()
    {
        List<DocumentReference> references = new();

        foreach (var id in Ids)
        {
            references.Add(Parent.Document(id));
        }

        return references.AsReadOnly();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is MultipleDocumentReference reference &&
               base.Equals(obj) &&
               EqualityComparer<Database>.Default.Equals(Database, reference.Database) &&
               EqualityComparer<IEnumerable<string>>.Default.Equals(Ids, reference.Ids) &&
               EqualityComparer<CollectionReference>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 952618460;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Database>.Default.GetHashCode(Database);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<string>>.Default.GetHashCode(Ids);
        hashCode = hashCode * -1521134295 + EqualityComparer<CollectionReference>.Default.GetHashCode(Parent);
        return hashCode;
    }

    internal override string[] BuildUrls(string projectId, string? postSegment = null)
    {
        return GetDocumentReferences().Select(i => i.BuildUrl(projectId, postSegment)).ToArray();
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
