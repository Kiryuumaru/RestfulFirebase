namespace RestfulFirebase.CloudFirestore.Query;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a parameter in firebase query, e.g. "?data=foo".
/// </summary>
public abstract class ParameterQuery : FirebaseQuery
{
    #region Properties

    private readonly Func<string> parameterFactory;
    private readonly string separator;

    #endregion

    #region Initializers

    private protected ParameterQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> parameterFactory)
        : base(app, parent)
    {
        this.parameterFactory = parameterFactory;
        separator = (Parent is ChildQuery) ? "?" : "&";
    }

    #endregion

    #region Methods

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(Func<string> valueFactory)
    {
        return new FilterQuery(App, this, () => "startAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory"> Value to end at. </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(Func<string> valueFactory)
    {
        return new FilterQuery(App, this, () => "endAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(Func<string?> valueFactory)
    {
        return new FilterQuery(App, this, () => "equalTo", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(Func<double> valueFactory)
    {
        return new FilterQuery(App, this, () => "startAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(Func<double> valueFactory)
    {
        return new FilterQuery(App, this, () => "endAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(Func<double> valueFactory)
    {
        return new FilterQuery(App, this, () => "equalTo", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(Func<long> valueFactory)
    {
        return new FilterQuery(App, this, () => "startAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(Func<long> valueFactory)
    {
        return new FilterQuery(App, this, () => "endAt", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(Func<long> valueFactory)
    {
        return new FilterQuery(App, this, () => "equalTo", valueFactory);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="valueFactory">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(Func<bool> valueFactory)
    {
        return new FilterQuery(App, this, () => "equalTo", valueFactory);
    }

    /// <summary>
    /// Limits the result to first <paramref name="countFactory"/> items.
    /// </summary>
    /// <param name="countFactory">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery LimitToFirst(Func<int> countFactory)
    {
        return new FilterQuery(App, this, () => "limitToFirst", () => countFactory());
    }

    /// <summary>
    /// Limits the result to last <paramref name="countFactory"/> items.
    /// </summary>
    /// <param name="countFactory">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery LimitToLast(Func<int> countFactory)
    {
        return new FilterQuery(App, this, () => "limitToLast", () => countFactory());
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(string value)
    {
        return StartAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(string value)
    {
        return EndAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(string value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(double value)
    {
        return StartAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(double value)
    {
        return EndAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(double value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to start at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery StartAt(long value)
    {
        return StartAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to end at.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EndAt(long value)
    {
        return EndAt(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(long value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <param name="value">
    /// Value to equal to.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo(bool value)
    {
        return EqualTo(() => value);
    }

    /// <summary>
    /// Instructs firebase to send data equal to <c>null</c>. This must be preceded by an OrderBy query.
    /// </summary>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery EqualTo()
    {
        return EqualTo(() => null);
    }

    /// <summary>
    /// Limits the result to first <paramref name="count"/> items.
    /// </summary>
    /// <param name="count">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery LimitToFirst(int count)
    {
        return LimitToFirst(() => count);
    }

    /// <summary>
    /// Limits the result to last <paramref name="count"/> items.
    /// </summary>
    /// <param name="count">
    /// Number of elements.
    /// </param>
    /// <returns>
    /// The created <see cref="FilterQuery"/>.
    /// </returns>
    public FilterQuery LimitToLast(int count)
    {
        return LimitToLast(() => count);
    }

    /// <summary>
    /// Builds the URL parameter of the query.
    /// </summary>
    /// <returns>
    /// The built URL parameter of the query.
    /// </returns>
    protected abstract string BuildUrlParameter();

    /// <summary>
    /// Builds the URL parameter of the query.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/> that represents the built url parameter of the query.
    /// </returns>
    protected abstract Task<string> BuildUrlParameterAsync();

    #endregion

    #region FirebaseQuery Members

    /// <inheritdoc/>
    protected override string BuildUrlSegment(IFirebaseQuery child)
    {
        return $"{separator}{parameterFactory()}={BuildUrlParameter()}";
    }

    /// <inheritdoc/>
    protected override async Task<string> BuildUrlSegmentAsync(IFirebaseQuery? child)
    {
        return $"{separator}{parameterFactory()}={await BuildUrlParameterAsync()}";
    }

    #endregion

    #region IFirebaseQuery Members

    /// <inheritdoc/>
    public override string GetAbsoluteUrl()
    {
        return Parent?.BuildUrl(this) ?? base.GetAbsoluteUrl();
    }

    #endregion
}
