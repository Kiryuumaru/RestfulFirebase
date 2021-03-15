using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Query extensions providing linq like syntax for firebase server methods.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Adds an auth parameter to the query.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="token"> The auth token. </param>
        /// <returns> The <see cref="AuthQuery"/>. </returns>
        internal static AuthQuery WithAuth(this FirebaseQuery node, string token)
        {
            return node.WithAuth(() => token);
        }

        /// <summary>
        /// Appends print=silent to save bandwidth.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <returns> The <see cref="SilentQuery"/>. </returns>
        internal static SilentQuery Silent(this FirebaseQuery node)
        {
            return new SilentQuery(node, node.App);
        }

        /// <summary>
        /// Appends shallow=true to the url parameters. This cannot be used with any other filtering parameters.
        /// See https://firebase.google.com/docs/database/rest/retrieve-data
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <returns> The <see cref="ShallowQuery"/>. </returns>
        public static ShallowQuery Shallow(this ChildQuery node)
        {
            return new ShallowQuery(node, node.App);
        }

        /// <summary>
        /// References a sub child of the existing node.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="path"> The path of sub child. </param>
        /// <returns> The <see cref="ChildQuery"/>. </returns>
        public static ChildQuery Child(this ChildQuery node, string path)
        {
            return node.Child(() => path);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyName"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderBy(this ChildQuery child, string propertyName)
        {
            return child.OrderBy(() => propertyName);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, string value)
        {
            return child.StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, string value)
        {
            return child.EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, string value)
        {
            return child.EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, double value)
        {
            return child.StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, double value)
        {
            return child.EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, double value)
        {
            return child.EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, long value)
        {
            return child.StartAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, long value)
        {
            return child.EndAt(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, long value)
        {
            return child.EqualTo(() => value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <paramref name="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, bool value)
        {
            return child.EqualTo(() => value);
        }  

        /// <summary>
        /// Instructs firebase to send data equal to null. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child)
        {
            return child.EqualTo(() => null);
        }        

        /// <summary>
        /// Limits the result to first <paramref name="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToFirst(this ParameterQuery child, int count)
        {
            return child.LimitToFirst(() => count);
        }

        /// <summary>
        /// Limits the result to last <paramref name="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToLast(this ParameterQuery child, int count)
        {
            return child.LimitToLast(() => count);
        }

        public static Task PutAsync<T>(this FirebaseQuery query, T obj)
        {
            return query.PutAsync(JsonConvert.SerializeObject(obj, query.App.Config.JsonSerializerSettings));
        }

        public static Task PatchAsync<T>(this FirebaseQuery query, T obj)
        {
            return query.PatchAsync(JsonConvert.SerializeObject(obj, query.App.Config.JsonSerializerSettings));
        }

        public static async Task<FirebaseObject<T>> PostAsync<T>(this FirebaseQuery query, T obj, bool generateKeyOffline = true)
        {
            var result = await query.PostAsync(JsonConvert.SerializeObject(obj, query.App.Config.JsonSerializerSettings), generateKeyOffline).ConfigureAwait(false);

            return new FirebaseObject<T>(result.Key, obj);
        }

        /// <summary>
        /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
        /// </summary>
        /// <typeparam name="T"> Type of object to fan out. </typeparam>
        /// <param name="query"> Current node. </param>
        /// <param name="item"> Object to fan out. </param>
        /// <param name="relativePaths"> Locations where to store the item. </param>
        public static Task FanOut<T>(this ChildQuery query, T item, params string[] relativePaths)
        {
            if (relativePaths == null)
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }

            var fanoutObject = new Dictionary<string, T>(relativePaths.Length);

            foreach (var path in relativePaths)
            {
                fanoutObject.Add(path, item);
            }

            return query.PatchAsync(fanoutObject);
        }
    }
}
