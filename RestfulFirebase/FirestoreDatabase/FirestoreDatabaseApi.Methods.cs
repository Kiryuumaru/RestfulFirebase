using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
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

        return new CollectionReference(App, collectionId, null);
    }

    /// <summary>
    /// Creates a root collection group reference <see cref="CollectionGroupReference"/>.
    /// </summary>
    /// <param name="collectionIds">
    /// The multiple ID of the collection reference.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionIds"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionIds"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionGroupReference CollectionGroup(params string[] collectionIds)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        return new CollectionGroupReference(App, collectionIds, null);
    }
}
