using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The reference for collections.
/// </summary>
public class CollectionReference : Reference
{
    #region Properties

    /// <summary>
    /// Gets the ID of the collection reference.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the collection reference.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    #endregion

    #region Initializers

    internal CollectionReference(RestfulFirebaseApp app, FirestoreDatabase firestoreDatabase, DocumentReference? parent, string collectionId)
        : base(app, firestoreDatabase)
    {
        Id = collectionId;
        Parent = parent;

        if (parent == null)
        {
            Name = $"projects/{App.Config.ProjectId}/databases/{firestoreDatabase.DatabaseId}/documents/{Id}";
        }
        else
        {
            Name = $"{parent.Name}/{collectionId}";
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a document reference <see cref="DocumentReference"/>.
    /// </summary>
    /// <param name="documentId">
    /// The ID of the document reference.
    /// </param>
    /// <returns>
    /// The <see cref="DocumentReference"/> of the specified <paramref name="documentId"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentId"/> is a <c>null</c> reference.
    /// </exception>
    public DocumentReference Document(string documentId)
    {
        if (documentId == null)
        {
            throw new ArgumentNullException(nameof(documentId));
        }

        return new DocumentReference(App, Database, this, documentId);
    }

    internal override string BuildUrl()
    {
        var url = BuildUrlSegment();

        if (Parent == null)
        {
            url = string.Format(
                CloudFirestoreApp.CloudFirestoreDocumentsEndpoint,
                App.Config.ProjectId,
                Database.DatabaseId,
                url);
        }
        else
        {
            string parentUrl = Parent.BuildUrl();
            if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
            {
                parentUrl += '/';
            }
            url = parentUrl + url;
        }

        return url;
    }

    internal override string BuildUrlSegment()
    {
        return Id;
    }

    #endregion
}
