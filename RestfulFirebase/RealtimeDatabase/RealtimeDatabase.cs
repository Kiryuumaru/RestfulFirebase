using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.IO;
using System.Linq;

namespace RestfulFirebase.RealtimeDatabase;

/// <summary>
/// App module that provides firebase realtime database implementations.
/// </summary>
public class RealtimeDatabase
{
    /// <summary>
    /// Gets the database URL used by this instance.
    /// </summary>
    public string DatabaseUrl { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used by this instance.
    /// </summary>
    internal FirebaseApp App { get; }

    internal RealtimeDatabase(FirebaseApp app, string databaseUrl)
    {
        databaseUrl = databaseUrl.Trim().Trim('/');

        App = app;
        DatabaseUrl = databaseUrl;
    }

    /// <summary>
    /// Creates new instance of <see cref="Reference"/> node with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="Reference"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is empty or has forbidden character.
    /// </exception>
    public Reference Child(string path)
    {
        return Reference.Create(this, null, path);
    }
}
