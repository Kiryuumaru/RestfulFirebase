using System;
using System.Linq;

namespace RestfulFirebase.RealtimeDatabase.References;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public partial class Reference
{
    /// <summary>
    /// Gets the <see cref="RestfulFirebase.RealtimeDatabase.RealtimeDatabase"/> used.
    /// </summary>
    public RealtimeDatabase RealtimeDatabase { get; }

    /// <summary>
    /// Gets the parent of the query.
    /// </summary>
    public Reference? Parent { get; }

    /// <summary>
    /// Gets the path of the reference.
    /// </summary>
    public string[] Path { get; }

    /// <summary>
    /// Gets the URL of the reference.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal Reference(RealtimeDatabase realtimeDatabase, Reference? parent, string? segement)
    {
        App = realtimeDatabase.App;
        RealtimeDatabase = realtimeDatabase;
        Parent = parent;

        if (segement == null || string.IsNullOrEmpty(segement))
        {
            Path = Array.Empty<string>();
            Url = $"{realtimeDatabase.DatabaseUrl}";
        }
        else if (parent == null)
        {
            Path = new string[] { segement };
            Url = $"{realtimeDatabase.DatabaseUrl}/{segement}";
        }
        else
        {
            Path = new string[parent.Path.Length + 1];
            Array.Copy(parent.Path, Path, parent.Path.Length);
            Path[^1] = segement;
            Url = $"{parent.Url}/{segement}";
        }
    }
}
