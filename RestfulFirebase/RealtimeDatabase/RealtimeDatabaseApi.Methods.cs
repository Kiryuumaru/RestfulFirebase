namespace RestfulFirebase.RealtimeDatabase;

public partial class RealtimeDatabaseApi
{
    /// <summary>
    /// Creates new instance of <see cref="RealtimeDatabase"/> database with the specified <paramref name="databaseUrl"/>.
    /// </summary>
    /// <param name="databaseUrl">
    /// The URL of the database. Set to <c>null</c> if the instance will use the default firebase realtime database (i.e., "https://projectid-default-rtdb.firebaseio.com/").
    /// </param>
    /// <returns>
    /// The created <see cref="RealtimeDatabase"/> node.
    /// </returns>
    public RealtimeDatabase Database(string? databaseUrl = default)
    {
        if (databaseUrl == null || string.IsNullOrEmpty(databaseUrl))
        {
            databaseUrl = $"https://{App.Config.ProjectId}-default-rtdb.firebaseio.com";
        }

        return new RealtimeDatabase(App, databaseUrl);
    }
}
