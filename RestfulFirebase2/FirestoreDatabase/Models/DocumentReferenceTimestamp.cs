using System;
using RestfulFirebase.FirestoreDatabase.References;

namespace RestfulFirebase.FirestoreDatabase.Requests;

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

    internal DocumentReferenceTimestamp(DocumentReference reference, DateTimeOffset readTime)
    {
        Reference = reference;
        ReadTime = readTime;
    }
}
