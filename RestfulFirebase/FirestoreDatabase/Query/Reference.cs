using RestfulFirebase.FirestoreDatabase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.CloudFirestore.Query;

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



    #endregion
}
