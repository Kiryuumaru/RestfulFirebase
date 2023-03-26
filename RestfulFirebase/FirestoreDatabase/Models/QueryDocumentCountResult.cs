using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result for query count operation.
/// </summary>
public class QueryDocumentCountResult
{
    /// <summary>
    /// Gets the result count of documents.
    /// </summary>
    public long Count { get; internal set; }

    /// <summary>
    /// Gets the time at which the documents was read.
    /// </summary>
    public DateTimeOffset ReadTime { get; internal set; }

    internal QueryDocumentCountResult(long count, DateTimeOffset readTime)
    {
        Count = count;
        ReadTime = readTime;
    }
}
