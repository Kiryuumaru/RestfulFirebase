using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.References;

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

    /// <summary>
    /// Creates new instance of <see cref="DocumentReference"/>.
    /// </summary>
    /// <param name="parent">
    /// The <see cref="DocumentReference"/> parent of the document to create.
    /// </param>
    /// <param name="documentId">
    /// The document ID of the document to create.
    /// </param>
    /// <returns>
    /// The created <see cref="DocumentReference"/>.
    /// </returns>
    public static DocumentReference Create(CollectionReference parent, string documentId)
    {
        return new(parent, documentId);
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static DocumentReference? Parse(string? json)
    {
        if (json != null && !string.IsNullOrEmpty(json))
        {
            string[] paths = json.Split('/');
            object currentPath = Api.FirestoreDatabase.Collection(paths[5]);

            for (int i = 6; i < paths.Length; i++)
            {
                if (currentPath is CollectionReference colPath)
                {
                    currentPath = colPath.Document(paths[i]);
                }
                else if (currentPath is DocumentReference docPath)
                {
                    currentPath = docPath.Collection(paths[i]);
                }
            }

            if (currentPath is DocumentReference documentReference)
            {
                return documentReference;
            }
        }

        return null;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static DocumentReference? Parse(JsonElement jsonElement, JsonSerializerOptions jsonSerializerOptions)
    {
        return Parse(jsonElement.Deserialize<string>(jsonSerializerOptions));
    }

    internal DocumentReference(CollectionReference parent, string documentId)
    {
        Id = documentId;
        Parent = parent;
    }

    #endregion

    #region Methods

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

        return new CollectionReference(this, collectionId);
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
    public Document<T> Create<T>(T? model = null)
        where T : class
    {
        return new Document<T>(this, model);
    }

    internal override string BuildUrlCascade(string projectId)
    {
        var url = Id;

        string parentUrl = Parent.BuildUrlCascade(projectId);
        if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
        {
            parentUrl += '/';
        }
        url = parentUrl + url;

        return url;
    }

    #endregion
}
