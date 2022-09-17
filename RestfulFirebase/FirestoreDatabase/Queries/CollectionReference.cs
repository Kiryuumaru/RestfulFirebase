using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Queries;

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
    /// Gets the parent of the collection reference. returns a <c>null</c> reference if the collection reference is a root collection reference.
    /// </summary>
    public DocumentReference? Parent { get; }

    #endregion

    #region Initializers

    internal CollectionReference(DocumentReference? parent, string collectionId)
    {
        Id = collectionId;
        Parent = parent;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CollectionReference reference &&
               base.Equals(obj) &&
               Id == reference.Id &&
               EqualityComparer<DocumentReference?>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
        hashCode = hashCode * -1521134295 + (Parent == null ? 0 : EqualityComparer<DocumentReference?>.Default.GetHashCode(Parent));
        return hashCode;
    }

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
        ArgumentNullException.ThrowIfNull(documentId);

        return new DocumentReference(this, documentId);
    }

    internal override string BuildUrlCascade(string projectId)
    {
        var url = Id;

        if (Parent == null)
        {
            url = string.Format(
                Api.FirestoreDatabase.FirestoreDatabaseDocumentsEndpoint,
                projectId,
                $"/{url}");
        }
        else
        {
            string parentUrl = Parent.BuildUrlCascade(projectId);
            if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
            {
                parentUrl += '/';
            }
            url = parentUrl + url;
        }

        return url;
    }

    #endregion
}
