using RestfulFirebase.FirestoreDatabase;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <summary>
    /// Creates an instance of <see cref="Database"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="RestfulFirebase.FirestoreDatabase.Database"/>.
    /// </returns>
    public static Database Database(string? databaseId = default)
    {
        return RestfulFirebase.FirestoreDatabase.Database.Get(databaseId);
    }
}
