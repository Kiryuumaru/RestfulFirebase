using RestfulFirebase.FirestoreDatabase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.Query;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract class Reference : Query
{
    #region Properties



    #endregion

    #region Initializers

    /// <summary>
    /// Created a new instance of <see cref="Reference"/>.
    /// </summary>
    /// <param name="database">
    /// The <see cref="Database"/> used by this instance.
    /// </param>
    public Reference(Database database) : base(database)
    {

    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Reference reference &&
               base.Equals(obj) &&
               EqualityComparer<Database>.Default.Equals(Database, reference.Database);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 365298542;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Database>.Default.GetHashCode(Database);
        return hashCode;
    }

    #endregion
}
