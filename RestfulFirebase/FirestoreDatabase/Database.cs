using RestfulFirebase.CloudFirestore.Query;

namespace RestfulFirebase.FirestoreDatabase;

/// <summary>
/// The database instance of the firestore.
/// </summary>
public class Database
{
    #region Properties

    /// <summary>
    /// Gets the database id of the firestore.
    /// </summary>
    public string DatabaseId { get; }

    #endregion

    #region Initializers

    internal Database(string databaseId)
    {
        DatabaseId = databaseId;
    }

    #endregion

    #region Methods

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

    #endregion
}
