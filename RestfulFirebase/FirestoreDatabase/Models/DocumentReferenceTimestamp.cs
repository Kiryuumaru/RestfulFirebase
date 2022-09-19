using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase;
using System.Transactions;
using RestfulFirebase.Common.Transactions;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.Common;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

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
