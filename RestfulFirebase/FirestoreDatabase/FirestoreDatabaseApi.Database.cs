using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.References;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
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
    public static CollectionReference Collection(string collectionId)
    {
        ArgumentNullException.ThrowIfNull(collectionId);

        return new CollectionReference(null, collectionId);
    }
}
