using RestfulFirebase.FirestoreDatabase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.FirestoreDatabase.References;

/// <summary>
/// The base reference of the cloud firestore.
/// </summary>
public abstract class Reference
{
    #region Properties



    #endregion

    #region Methods

    internal virtual string BuildUrl(string projectId, string? postSegment = null)
    {
        return $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/{BuildUrlCascade(projectId)}{postSegment}";
    }

    internal virtual string[] BuildUrls(string projectId, string? postSegment = null)
    {
        return new string[] { $"{Api.FirestoreDatabase.FirestoreDatabaseV1Endpoint}/{BuildUrlCascade(projectId)}{postSegment}" };
    }

    internal abstract string BuildUrlCascade(string projectId);


    #endregion
}
