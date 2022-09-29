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

    internal abstract string BuildUrlCascade(string projectId);


    #endregion
}
