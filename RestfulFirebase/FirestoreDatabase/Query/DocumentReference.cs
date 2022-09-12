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
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using System.Text.Json.Serialization;

namespace RestfulFirebase.CloudFirestore.Query;

/// <summary>
/// The reference for documents.
/// </summary>
public class DocumentReference : Reference
{
    #region Properties

    /// <summary>
    /// Gets the ID of the collection reference.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parent of the document reference.
    /// </summary>
    public CollectionReference Parent { get; }

    #endregion

    #region Initializers

    internal DocumentReference(Database database, CollectionReference parent, string documentId)
        : base(database)
    {
        Id = documentId;
        Parent = parent;
    }

    #endregion

    #region Methods

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

        return new CollectionReference(Database, this, collectionId);
    }

    internal override string BuildUrlCascade(string projectId)
    {
        var url = BuildUrlSegment(projectId);

        string parentUrl = Parent.BuildUrlCascade(projectId);
        if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
        {
            parentUrl += '/';
        }
        url = parentUrl + url;

        return url;
    }

    internal override string BuildUrlSegment(string projectId)
    {
        return Id;
    }

    #endregion
}
