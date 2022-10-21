using RestfulFirebase.FirestoreDatabase.Queries;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Creates a new query.
    /// </summary>
    /// <returns>
    /// The newly created <see cref="QueryRoot"/>.
    /// </returns>
    public QueryRoot Query()
    {
        return new QueryRoot(App, null, null);
    }

    /// <summary>
    /// Creates a new query.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <returns>
    /// The newly created <see cref="QueryRoot"/>.
    /// </returns>
    public QueryRoot Query<TModel>()
        where TModel : class
    {
        return new QueryRoot(App, typeof(TModel), null);
    }
}
