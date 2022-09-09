namespace RestfulFirebase.RealtimeDatabase.Query;

using System.Threading.Tasks;

/// <summary>
/// Appends shallow=true to the url.
/// </summary>
public class ShallowQuery : ParameterQuery
{
    #region Properties


    #endregion

    #region Initializers

    internal ShallowQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent)
        : base(realtimeDatabase, parent, () => "shallow")
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
