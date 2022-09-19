using DisposableHelpers;
using DisposableHelpers.Attributes;

namespace RestfulFirebase.Storage;

/// <summary>
/// App module that provides firebase storage implementations
/// </summary>
[Disposable]
public partial class StorageBucket
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets the storage bucket used by this instance.
    /// </summary>
    public string Bucket { get; }

    #endregion

    #region Initializers

    internal StorageBucket(RestfulFirebaseApp app, string bucket)
    {
        App = app;
        Bucket = bucket;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates new instance of <see cref="FirebaseStorageReference"/> child reference.
    /// </summary>
    /// <param name="childRoot">
    /// The child reference name or file name.
    /// </param>
    /// <returns>
    /// The instance of <see cref="FirebaseStorageReference"/> child reference.
    /// </returns>
    public FirebaseStorageReference Child(string childRoot)
    {
        return new FirebaseStorageReference(this, childRoot);
    }

    #endregion
}
