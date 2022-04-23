namespace RestfulFirebase.CloudFirestore.Query;

using System.Threading.Tasks;

/// <summary>
/// Appends print=silent to the url.
/// </summary>
public class SilentQuery : ParameterQuery
{
    #region Properties


    #endregion

    #region Initializers

    internal SilentQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        : base(app, parent, () => "print")
    {

    }

    #endregion

    #region Methods


    #endregion

    #region ParameterQuery Members

    /// <inheritdoc/>
    protected override string BuildUrlParameter()
    {
        return "silent";
    }

    /// <inheritdoc/>
    protected override Task<string> BuildUrlParameterAsync()
    {
        return Task.FromResult(BuildUrlParameter());
    }

    #endregion
}
