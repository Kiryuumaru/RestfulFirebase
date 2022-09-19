using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Models;
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

    /// <summary>
    /// Creates a multiple document <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="documents">
    /// The documents to create
    /// </param>
    /// <returns>
    /// The <see cref="Document{T}"/>.
    /// </returns>
    public IEnumerable<Document<T>> CreateDocuments<T>(params (string documentName, T? model)[] documents)
        where T : class
    {
        List<Document<T>> docs = new();

        foreach (var (documentName, model) in documents)
        {
            docs.Add(Document(documentName).Create(model));
        }

        return docs.AsReadOnly();
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
