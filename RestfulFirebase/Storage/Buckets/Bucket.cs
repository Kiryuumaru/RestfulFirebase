namespace RestfulFirebase.Storage.Buckets;

/// <summary>
/// App module that provides firebase storage implementations
/// </summary>
public partial class Bucket
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used by this instance.
    /// </summary>
    public FirebaseApp App { get; }

    /// <summary>
    /// Gets the storage bucket name used by this instance.
    /// </summary>
    public string Name { get; }

    internal Bucket(FirebaseApp app, string name)
    {
        App = app;
        Name = name;
    }
}
