using System;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// The timestamp of the document
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class DocumentTimestamp<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public Document<T> Document { get; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> time at which the document was read.
    /// </summary>
    public DateTimeOffset ReadTime { get; }

    internal DocumentTimestamp(Document<T> document, DateTimeOffset readTime)
    {
        Document = document;
        ReadTime = readTime;
    }
}
