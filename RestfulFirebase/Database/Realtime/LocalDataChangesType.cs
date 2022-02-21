using RestfulFirebase.Utilities;

namespace RestfulFirebase.Database.Realtime;

/// <summary>
/// Enumeration of types of local data changes.
/// </summary>
public enum LocalDataChangesType
{
    /// <summary>
    /// Data is synced on both local and online database.
    /// </summary>
    Synced,

    /// <summary>
    /// Data is created locally and is not present yet on online database.
    /// </summary>
    Create,

    /// <summary>
    /// Data is updated locally and is not updated yet on online database.
    /// </summary>
    Update,

    /// <summary>
    /// Data is deleted locally and is not deleted yet on online database.
    /// </summary>
    Delete
}
