using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Request a transaction to start an atomic operation that can only be used for read operations.
    /// </summary>
    /// <param name="readTime">
    /// The given time documents will read. This may not be older than 60 seconds.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="ReadOnlyTransaction"/>.
    /// </returns>
    public Task<HttpResponse<ReadOnlyTransaction>> BeginReadOnlyTransaction(DateTimeOffset? readTime = null, IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        return ExecuteBeginTransaction(new ReadOnlyTransaction(readTime), authorization, cancellationToken);
    }

    /// <summary>
    /// Request a transaction to start an atomic operation that can only be used for read operations.
    /// </summary>
    /// <param name="retryTransaction">
    /// An optional transaction to retry.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the newly created <see cref="ReadOnlyTransaction"/>.
    /// </returns>
    public Task<HttpResponse<ReadWriteTransaction>> BeginReadWriteTransaction(string? retryTransaction, IAuthorization? authorization = null, CancellationToken cancellationToken = default)
    {
        return ExecuteBeginTransaction(new ReadWriteTransaction(retryTransaction), authorization, cancellationToken);
    }
}
