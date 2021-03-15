namespace RestfulFirebase.Database.Query
{
    using System;

    /// <summary>
    /// Query extensions providing linq like syntax for firebase server methods.
    /// </summary>
    public static class QueryFactoryExtensions
    {
        /// <summary>
        /// Adds an auth parameter to the query.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="tokenFactory"> The auth token. </param>
        /// <returns> The <see cref="AuthQuery"/>. </returns>
        internal static AuthQuery WithAuth(this FirebaseQuery node, Func<string> tokenFactory)
        {
            return new AuthQuery(node, tokenFactory, node.App);
        }

        /// <summary>
        /// References a sub child of the existing node.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="pathFactory"> The path of sub child. </param>
        /// <returns> The <see cref="ChildQuery"/>. </returns>
        public static ChildQuery Child(this ChildQuery node, Func<string> pathFactory)
        {
            return new ChildQuery(node, pathFactory, node.App);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyNameFactory"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <param name="propertyNameFactory"> The property name. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderBy(this ChildQuery child, Func<string> propertyNameFactory)
        {
            return new OrderQuery(child, propertyNameFactory, child.App);
        }

        /// <summary>
        /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByKey(this ChildQuery child)
        {
            return child.OrderBy("$key");
        }

        /// <summary>
        /// Order data by $value. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByValue(this ChildQuery child)
        {
            return child.OrderBy("$value");
        }

        /// <summary>
        /// Order data by $priority. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByPriority(this ChildQuery child)
        {
            return child.OrderBy("$priority");
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, Func<string> valueFactory)
        {
            return new FilterQuery(child, () => "startAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, Func<string> valueFactory)
        {
            return new FilterQuery(child, () => "endAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, Func<string> valueFactory)
        {
            return new FilterQuery(child, () => "equalTo", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, Func<double> valueFactory)
        {
            return new FilterQuery(child, () => "startAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, Func<double> valueFactory)
        {
            return new FilterQuery(child, () => "endAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, Func<double> valueFactory)
        {
            return new FilterQuery(child, () => "equalTo", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, Func<long> valueFactory)
        {
            return new FilterQuery(child, () => "startAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, Func<long> valueFactory)
        {
            return new FilterQuery(child, () => "endAt", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, Func<long> valueFactory)
        {
            return new FilterQuery(child, () => "equalTo", valueFactory, child.App);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="valueFactory"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="valueFactory"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, Func<bool> valueFactory)
        {
            return new FilterQuery(child, () => "equalTo", valueFactory, child.App);
        }

        /// <summary>
        /// Limits the result to first <paramref name="countFactory"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="countFactory"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToFirst(this ParameterQuery child, Func<int> countFactory)
        {
            return new FilterQuery(child, () => "limitToFirst", () => countFactory(), child.App);
        }

        /// <summary>
        /// Limits the result to last <paramref name="countFactory"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="countFactory"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToLast(this ParameterQuery child, Func<int> countFactory)
        {
            return new FilterQuery(child, () => "limitToLast", () => countFactory(), child.App);
        }
    }
}
