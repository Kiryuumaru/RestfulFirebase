using System;
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
public class GetDocumentResult
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public DocumentTimestamp? Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public DocumentReferenceTimestamp? Missing { get; }

    internal GetDocumentResult(DocumentTimestamp? found, DocumentReferenceTimestamp? missing)
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
public class GetDocumentResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public DocumentTimestamp<T>? Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public DocumentReferenceTimestamp? Missing { get; }

    internal GetDocumentResult(DocumentTimestamp<T>? found, DocumentReferenceTimestamp? missing)
    {
        Found = found;
        Missing = missing;
    }
}
