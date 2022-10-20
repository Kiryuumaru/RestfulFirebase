using System;
using RestfulFirebase.FirestoreDatabase.References;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The reference timestamp of the document.
/// </summary>
public class DocumentReferenceTimestamp
{
    /// <summary>
    /// Gets the reference of the document.
    /// </summary>
    public DocumentReference Reference { get; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> time at which the document was read.
    /// </summary>
    public DateTimeOffset ReadTime { get; }

    /// <summary>
    /// Gets <c>true</c> whether the <see cref="ReadTime"/> is a server time; otherwise, <c>false</c>.
    /// </summary>
    public bool IsReadTimeAServerTime { get; }

    internal DocumentReferenceTimestamp(DocumentReference reference, DateTimeOffset readTime, bool isReadTimeAServerTime)
    {
        Reference = reference;
        ReadTime = readTime;
        IsReadTimeAServerTime = isReadTimeAServerTime;
    }
}
