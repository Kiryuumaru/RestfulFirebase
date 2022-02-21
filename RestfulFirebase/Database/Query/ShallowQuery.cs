using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query;

/// <summary>
/// Appends shallow=true to the url.
/// </summary>
public class ShallowQuery : ParameterQuery
{
    #region Properties


    #endregion

    #region Initializers

    internal ShallowQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        : base(app, parent, () => "shallow")
    {

    }

    #endregion

    #region Methods


    #endregion

    #region ParameterQuery Members

    /// <inheritdoc/>
    protected override string BuildUrlParameter()
    {
        return "true";
    }

    /// <inheritdoc/>
    protected override Task<string> BuildUrlParameterAsync()
    {
        return Task.FromResult(BuildUrlParameter());
    }

    #endregion
}
