using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public partial class Query
{
    /// <summary>
    /// Runs the structured query.
    /// </summary>
    /// <param name="cacheDocuments">
    /// The cached <see cref="Document"/> that will use to populate the instance of the documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="QueryDocumentResult"/>.
    /// </returns>
    public Task<HttpResponse<QueryDocumentResult>> RunQuery(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.QueryDocument(this, cacheDocuments, transaction, authorization, cancellationToken);
    }
}

public partial class Query<TModel>
{
    /// <summary>
    /// Runs the structured query.
    /// </summary>
    /// <param name="cacheDocuments">
    /// The cached <see cref="Document"/> that will use to populate the instance of the documents.
    /// </param>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="QueryDocumentResult{TModel}"/>.
    /// </returns>
    public Task<HttpResponse<QueryDocumentResult<TModel>>> RunQuery(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.QueryDocument<TModel, Query<TModel>>(this, cacheDocuments, transaction, authorization, cancellationToken);
    }
}
