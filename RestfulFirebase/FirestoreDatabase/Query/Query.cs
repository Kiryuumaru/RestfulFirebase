using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.FirestoreDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.CloudFirestore.Query;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract class Query
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="Database"/> used by this instance.
    /// </summary>
    public Database Database { get; }

    #endregion

    #region Initializers

    internal Query(Database database)
    {
        Database = database;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Query query &&
               EqualityComparer<Database>.Default.Equals(Database, query.Database);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return 732662424 + EqualityComparer<Database>.Default.GetHashCode(Database);
    }

    internal string BuildUrl(string projectId)
    {
        return $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}{BuildUrlCascade(projectId)}";
    }

    internal abstract string BuildUrlCascade(string projectId);

    internal abstract string BuildUrlSegment(string projectId);

    #endregion
}
