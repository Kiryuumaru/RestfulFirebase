﻿using System;

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

    internal DocumentTimestamp(Document document, DateTimeOffset readTime)
    {
        Document = document;
        ReadTime = readTime;
    }
}

/// <summary>
/// The timestamp of the document.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class DocumentTimestamp<T> : DocumentTimestamp
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public new Document<T> Document { get; }

    internal DocumentTimestamp(Document<T> document, DateTimeOffset readTime)
        : base(document, readTime)
    {
        Document = document;
    }
}
