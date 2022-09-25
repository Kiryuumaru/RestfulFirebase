namespace RestfulFirebase.FirestoreDatabase.Enums;

/// <summary>
/// The value that is calculated by the server.
/// </summary>
public enum ServerValue
{
    /// <summary>
    /// The time at which the server processed the request, with millisecond precision. If used on multiple fields (same or different documents) in a transaction, all the fields will get the same server timestamp.
    /// </summary>
    RequestTime,

    /// <summary>
    /// Unspecified. This value must not be used.
    /// </summary>
    ServerValueUnspecified,

}
