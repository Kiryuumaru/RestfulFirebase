using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Collections.Generic;
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
    /// <exception cref="System.ArgumentException">
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
    /// <exception cref="System.ArgumentException">
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

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TQuery Cache(params Document[] documents)
    {
        if (documents != null)
        {
            WritableCacheDocuments.AddRange(documents);
        }

        return (TQuery)this;
    }

    /// <summary>
    /// Adds a cache documents.
    /// </summary>
    /// <param name="documents">
    /// The cache documents.
    /// </param>
    /// <returns>
    /// The request with new added cache documents.
    /// </returns>
    public TQuery Cache(IEnumerable<Document>? documents)
    {
        if (documents != null)
        {
            WritableCacheDocuments.AddRange(documents);
        }

        return (TQuery)this;
    }

    /// <summary>
    /// Sets the <see cref="Transactions.Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    /// <returns>
    /// The request with new added transaction.
    /// </returns>
    public TQuery Transaction(Transaction? transaction)
    {
        TransactionUsed = transaction;

        return (TQuery)this;
    }

    /// <summary>
    /// Sets the <see cref="Query.AuthorizationUsed"/> by the request.
    /// </summary>
    /// <returns>
    /// The request with new added authorization.
    /// </returns>
    public TQuery Authorization(IAuthorization? authorization)
    {
        AuthorizationUsed = authorization;

        return (TQuery)this;
    }

    /// <summary>
    /// Runs the structured query.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="QueryDocumentResult"/>.
    /// </returns>
    public async Task<HttpResponse<QueryDocumentResult>> Run(CancellationToken cancellationToken = default)
    {
        if (FromQuery.Count == 0)
        {
            return new();
        }

        JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

        return await QueryDocumentPage(
            new(),
            BuildStartingStructureQuery(jsonSerializerOptions, cancellationToken),
            0,
            PagesToSkip * SizeOfPages,
            jsonSerializerOptions,
            cancellationToken);
    }
}

public abstract partial class FluentQueryRoot<TQuery, TModel>
{
    /// <summary>
    /// Runs the structured query.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the created result <see cref="QueryDocumentResult{TModel}"/>.
    /// </returns>
    public new async Task<HttpResponse<QueryDocumentResult<TModel>>> Run(CancellationToken cancellationToken = default)
    {
        if (FromQuery.Count == 0)
        {
            return new();
        }

        JsonSerializerOptions jsonSerializerOptions = App.FirestoreDatabase.ConfigureJsonSerializerOption();

        return await QueryDocumentPage<TModel>(
            new(),
            BuildStartingStructureQuery(jsonSerializerOptions, cancellationToken),
            0,
            PagesToSkip * SizeOfPages,
            jsonSerializerOptions,
            cancellationToken);
    }
}
