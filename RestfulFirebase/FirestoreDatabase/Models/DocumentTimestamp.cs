using System;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The timestamp of the document.
/// </summary>
public class DocumentTimestamp
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> time at which the document was read.
    /// </summary>
    public DateTimeOffset ReadTime { get; }

    /// <summary>
    /// Gets <c>true</c> whether the <see cref="ReadTime"/> is a server time; otherwise, <c>false</c>.
    /// </summary>
    public bool IsReadTimeAServerTime { get; }

    internal DocumentTimestamp(Document document, DateTimeOffset readTime, bool isReadTimeAServerTime)
    {
        Document = document;
        ReadTime = readTime;
        IsReadTimeAServerTime = isReadTimeAServerTime;
    }
}

/// <summary>
/// The timestamp of the document.
/// </summary>
/// <typeparam name="TModel">
/// The type of the model of the document.
/// </typeparam>
public class DocumentTimestamp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : DocumentTimestamp
    where TModel : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public new Document<TModel> Document { get; }

    internal DocumentTimestamp(Document<TModel> document, DateTimeOffset readTime, bool isReadTimeAServerTime)
        : base(document, readTime, isReadTimeAServerTime)
    {
        Document = document;
    }
}
