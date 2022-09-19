namespace RestfulFirebase.FirestoreDatabase.References;

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
    /// <param name="app">
    /// The <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </param>
    /// <param name="database">
    /// The <see cref="FirestoreDatabase"/> used by this instance.
    /// </param>
    public Reference(RestfulFirebaseApp app, FirestoreDatabase database) : base(app, database)
    {

    }

    #endregion

    #region Methods



    #endregion
}
