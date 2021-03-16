namespace RestfulFirebase.Database.Query
{
    using System;

    /// <summary>
    /// Represents a parameter in firebase query, e.g. "?data=foo".
    /// </summary>
    public abstract class ParameterQuery : FirebaseQuery
    {
        private readonly Func<string> parameterFactory;
        private readonly string separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent of this query. </param>
        /// <param name="parameterFactory"> The parameter. </param>
        /// <param name="app"> The owner. </param>
        protected ParameterQuery(FirebaseQuery parent, Func<string> parameterFactory, RestfulFirebaseApp app)
            : base(parent, app)
        {
            this.parameterFactory = parameterFactory;
            separator = (Parent is ChildQuery) ? "?" : "&";
        }

        /// <summary>
        /// Build the url segment represented by this query. 
        /// </summary> 
        /// <param name="child"> The  </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            return $"{separator}{parameterFactory()}={BuildUrlParameter(child)}";
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The  </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected abstract string BuildUrlParameter(FirebaseQuery child);

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(Func<bool> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        /// <summary>
        /// Limits the result to first <paramref name="countFactory"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="countFactory"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery LimitToFirst(Func<int> countFactory)
        {
            return new FilterQuery(this, () => "limitToFirst", () => countFactory(), App);
        }

        /// <summary>
        /// Limits the result to last <paramref name="countFactory"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="countFactory"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery LimitToLast(Func<int> countFactory)
        {
            return new FilterQuery(this, () => "limitToLast", () => countFactory(), App);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(string value)
        {
            return StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(string value)
        {
            return EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(string value)
        {
            return EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(double value)
        {
            return StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(double value)
        {
            return EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(double value)
        {
            return EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery StartAt(long value)
        {
            return StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EndAt(long value)
        {
            return EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(long value)
        {
            return EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo(bool value)
        {
            return EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to null. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery EqualTo()
        {
            return EqualTo(() => null);
        }

        /// <summary>
        /// Limits the result to first <paramref name="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery LimitToFirst(int count)
        {
            return LimitToFirst(() => count);
        }

        /// <summary>
        /// Limits the result to last <paramref name="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public FilterQuery LimitToLast(int count)
        {
            return LimitToLast(() => count);
        }
    }
}
