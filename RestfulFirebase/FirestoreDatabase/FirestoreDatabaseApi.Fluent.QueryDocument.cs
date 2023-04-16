using RestfulFirebase.FirestoreDatabase.Queries;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Creates a new query.
    /// </summary>
    /// <returns>
    /// The newly created <see cref="Queries.Query"/>.
    /// </returns>
    public Query Query()
    {
        return new Query(App, null, null);
    }

    /// <summary>
    /// Creates a new query.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the document model.
    /// </typeparam>
    /// <returns>
    /// The newly created <see cref="Queries.Query"/>.
    /// </returns>
    public Query Query<TModel>()
        where TModel : class
    {
        return new Query(App, typeof(TModel), null);
    }
}
