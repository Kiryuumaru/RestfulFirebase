namespace RestfulFirebase.RealtimeDatabase.Query;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a firebase ordering query, e.g. "?OrderBy=Foo".
/// </summary>
public class OrderQuery : ParameterQuery
{
    #region Properties

    private readonly Func<string> propertyNameFactory;

    #endregion

    #region Initializers

    internal OrderQuery(RealtimeDatabase realtimeDatabase, ChildQuery parent, Func<string> propertyNameFactory)
        : base(realtimeDatabase, parent, () => "orderBy")
    {
        this.propertyNameFactory = propertyNameFactory;
    }

    #endregion

    #region Methods


    #endregion

    #region ParameterQuery Members

    /// <inheritdoc/>
    protected override string BuildUrlParameter()
    {
        return $"\"{propertyNameFactory()}\"";
    }

    /// <inheritdoc/>
    protected override Task<string> BuildUrlParameterAsync()
    {
        return Task.FromResult(BuildUrlParameter());
    }

    #endregion
}
