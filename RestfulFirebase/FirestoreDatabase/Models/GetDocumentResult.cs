﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.Json;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Transactions;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result of the <see cref="FirestoreDatabaseApi.GetDocument(Document.Builder, Transaction?, IAuthorization?, JsonSerializerOptions?, CancellationToken)"/> request.
/// </summary>
public class GetDocumentsResult
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentsResult(IReadOnlyList<DocumentTimestamp> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}

/// <summary>
/// The result of the <see cref="FirestoreDatabaseApi.GetDocument{T}(Document{T}.Builder, Transaction?, IAuthorization?, JsonSerializerOptions?, CancellationToken)"/> request.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class GetDocumentsResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentsResult(IReadOnlyList<DocumentTimestamp<T>> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}