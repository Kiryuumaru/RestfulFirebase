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

public abstract partial class FluentQueryRoot<TQuery>
{
    /// <summary>
    /// Sets the requested page size of pager async enumerator. Must be >= 1 if specified. Default is 20.
    /// </summary>
    /// <param name="pageSize">
    /// The page size of pager async enumerator.
    /// </param>
    /// <returns>
    /// The query with custom page size configuration.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="pageSize"/> is less than or equal to 0.
    /// </exception>
    public TQuery PageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            ArgumentException.Throw($"\"{nameof(pageSize)}\" is less than or equal to zero.");
        }

        SizeOfPages = pageSize;

        return (TQuery)this;
    }

    /// <summary>
    /// Sets the page to skip of pager async enumerator. Must be >= 0 if specified. Default is 0.
    /// </summary>
    /// <param name="skipPage">
    /// The page to skip of pager async enumerator
    /// </param>
    /// <returns>
    /// The query with custom skip page configuration.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="skipPage"/> is less than 0.
    /// </exception>
    public TQuery SkipPage(int skipPage)
    {
        if (skipPage < 0)
        {
            ArgumentException.Throw($"\"{nameof(skipPage)}\" is less than zero.");
        }

        PagesToSkip = skipPage;

        return (TQuery)this;
    }
}

public partial class QueryRoot
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
    public Task<HttpResponse<QueryDocumentResult>> Run(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.QueryDocument(this, cacheDocuments, transaction, authorization, cancellationToken);
    }
}

public partial class QueryRoot<TModel>
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
    public Task<HttpResponse<QueryDocumentResult<TModel>>> Run(
        IEnumerable<Document>? cacheDocuments = default,
        Transaction? transaction = default,
        IAuthorization? authorization = default,
        CancellationToken cancellationToken = default)
    {
        return App.FirestoreDatabase.QueryDocument<TModel>(this, cacheDocuments, transaction, authorization, cancellationToken);
    }
}
