using RestfulFirebase.RealtimeDatabase.Query;
using System;
using DisposableHelpers;

namespace RestfulFirebase.RealtimeDatabase;

/// <summary>
/// App module that provides firebase realtime database implementations.
/// </summary>
public class RealtimeDatabase : Disposable
{
    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets the database URL used by this instance.
    /// </summary>
    public string DatabaseUrl { get; }

    internal RealtimeDatabase(RestfulFirebaseApp app, string databaseUrl)
    {
        if (!databaseUrl.EndsWith("/"))
        {
            databaseUrl += "/";
        }

        App = app;
        DatabaseUrl = databaseUrl;
    }

    /// <summary>
    /// Creates new instance of <see cref="ChildQuery"/> node with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="ChildQuery"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="path"/> is null or empty.
    /// </exception>
    public ChildQuery Child(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        return new ChildQuery(this, null, path);
    }
}
