namespace RestfulFirebase.RealtimeDatabase.Query;

using System;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// Represents a firebase filtering query, e.g. "?LimitToLast=10".
/// </summary>
public class FilterQuery : ParameterQuery
{
    #region Properties

    private readonly Func<string?>? valueFactory;
    private readonly Func<double>? doubleValueFactory;
    private readonly Func<long>? longValueFactory;
    private readonly Func<bool>? boolValueFactory;

    #endregion

    #region Initializers

    internal FilterQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent, Func<string> filterFactory, Func<string?> valueFactory)
        : base(realtimeDatabase, parent, filterFactory)
    {
        this.valueFactory = valueFactory;
    }

    internal FilterQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory)
        : base(realtimeDatabase, parent, filterFactory)
    {
        doubleValueFactory = valueFactory;
    }

    internal FilterQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent, Func<string> filterFactory, Func<long> valueFactory)
        : base(realtimeDatabase, parent, filterFactory)
    {
        longValueFactory = valueFactory;
    }

    internal FilterQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent, Func<string> filterFactory, Func<bool> valueFactory)
        : base(realtimeDatabase, parent, filterFactory)
    {
        boolValueFactory = valueFactory;
    }

    #endregion

    #region Methods


    #endregion

    #region ParameterQuery Members

    /// <inheritdoc/>
    protected override string BuildUrlParameter()
    {
        if (valueFactory != null)
        {
            if (valueFactory() == null)
            {
                return $"null";
            }
            return $"\"{valueFactory()}\"";
        }
        else if (doubleValueFactory != null)
        {
            return doubleValueFactory().ToString(CultureInfo.InvariantCulture);
        }
        else if (longValueFactory != null)
        {
            return longValueFactory().ToString();
        }
        else if (boolValueFactory != null)
        {
            return $"{boolValueFactory().ToString().ToLower()}";
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    protected override Task<string> BuildUrlParameterAsync()
    {
        return Task.FromResult(BuildUrlParameter());
    }

    #endregion
}
