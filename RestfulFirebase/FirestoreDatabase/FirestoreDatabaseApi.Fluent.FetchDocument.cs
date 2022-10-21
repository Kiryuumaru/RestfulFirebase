using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.References;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Fetches;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Creates a new fetch.
    /// </summary>
    /// <returns>
    /// The newly created <see cref="FetchRoot"/>.
    /// </returns>
    public FetchRoot Fetch()
    {
        return new FetchRoot(App, null);
    }

    /// <summary>
    /// Creates a new fetch.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <returns>
    /// The newly created <see cref="FetchRoot"/>.
    /// </returns>
    public FetchRoot<TModel> Fetch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>()
        where TModel : class
    {
        return new FetchRoot<TModel>(App);
    }
}
