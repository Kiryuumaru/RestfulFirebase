using RestfulFirebase.Storage.References;

namespace RestfulFirebase.Storage.Buckets;

public partial class Bucket
{
    /// <summary>
    /// Creates new instance of <see cref="Reference"/> child reference.
    /// </summary>
    /// <param name="childRoot">
    /// The child reference name or file name.
    /// </param>
    /// <returns>
    /// The instance of <see cref="Reference"/> child reference.
    /// </returns>
    public Reference Child(string childRoot)
    {
        return new Reference(this, null, childRoot);
    }
}
