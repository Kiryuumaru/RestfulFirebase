using RestfulFirebase.FirestoreDatabase.Writes;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Creates a new write commit.
    /// </summary>
    /// <returns>
    /// The newly created <see cref="WriteRoot"/>.
    /// </returns>
    public WriteRoot Write()
    {
        return new WriteRoot(App);
    }
}
