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
    public Task<HttpResponse<QueryDocumentResult<TModel>>> RunQuery(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.QueryDocument<TModel, Query<TModel>>(this, cacheDocuments, transaction, authorization, cancellationToken);
    }
}
