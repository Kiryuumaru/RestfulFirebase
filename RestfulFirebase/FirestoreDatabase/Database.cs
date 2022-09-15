using RestfulFirebase.FirestoreDatabase.Query;

namespace RestfulFirebase.FirestoreDatabase;

/// <summary>
/// The database instance of the firestore.
/// </summary>
public class Database
{
    #region Properties

    /// <summary>
    /// Gets the database id of the firestore database.
    /// </summary>
    public string DatabaseId { get; }

    #endregion

    #region Initializers

    private Database(string? databaseId)
    {
        if (databaseId == null || string.IsNullOrEmpty(databaseId))
        {
            databaseId = "(default)";
        }

        DatabaseId = databaseId;
    }

    /// <summary>
    /// Creates an instance of <see cref="Database"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="Database"/>.
    /// </returns>
    public static Database Get(string? databaseId = default)
    {
        return new Database(databaseId);
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
