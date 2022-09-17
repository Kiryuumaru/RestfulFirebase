using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Queries;

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    /// <summary>
    /// Creates an instance of <see cref="Query"/> with the specified <paramref name="databaseId"/>
    /// </summary>
    /// <param name="databaseId">
    /// The ID of the database to use. Set to <c>null</c> if the instance will use the default database.
    /// </param>
    /// <returns>
    /// The created <see cref="Database"/>.
    /// </returns>
    public static Database Query(string? databaseId = default)
    {
        return Database.Query(databaseId);
    }
}
