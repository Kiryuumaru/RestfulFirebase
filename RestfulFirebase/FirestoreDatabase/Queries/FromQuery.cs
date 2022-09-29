using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The "from" parameter for query.
/// </summary>
public class FromQuery
{
    /// <summary>
    /// The builder for multiple document parameter.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of <see cref="Queries.FromQuery"/>.
        /// </summary>
        public List<FromQuery> FromQuery { get; } = new();

        /// <summary>
        /// Adds ascending order to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        public Builder Ascending(string propertyName)
        {
            FromQuery.Add(Queries.FromQuery.Ascending(propertyName));
            return this;
        }

        /// <summary>
        /// Adds descending order to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        public Builder Descending(string propertyName)
        {
            FromQuery.Add(Queries.FromQuery.Descending(propertyName));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The order based on the property name of the model to order.
        /// </param>
        /// <param name="orderDirection">
        /// The <see cref="Enums.OrderDirection"/> of the order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(string propertyName, OrderDirection orderDirection)
        {
            FromQuery.Add(Queries.FromQuery.Create(propertyName, orderDirection));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(FromQuery orderBy)
        {
            FromQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder AddRange(IEnumerable<FromQuery> orderBy)
        {
            FromQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to convert.
        /// </param>
        public static implicit operator Builder(FromQuery orderBy)
        {
            return Create().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> array to convert.
        /// </param>
        public static implicit operator Builder(FromQuery[] orderBy)
        {
            return Create().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> list to convert.
        /// </param>
        public static implicit operator Builder(List<FromQuery> orderBy)
        {
            return Create().AddRange(orderBy);
        }

        internal string BuildAsQueryParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(JsonSerializerOptions? jsonSerializerOptions)
        {
            return BuildAsQueryParameter(typeof(T), jsonSerializerOptions);
        }

        internal string BuildAsQueryParameter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, JsonSerializerOptions? jsonSerializerOptions)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            return BuildAsQueryParameter(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions);
        }

        internal string BuildAsQueryParameter(Type objType, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonSerializerOptions? jsonSerializerOptions)
        {
            List<string> orderByQuery = new();

            foreach (var order in FromQuery)
            {
                orderByQuery.Add($"{order.GetDocumentFieldName(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions)} {(order.OrderDirection == OrderDirection.Ascending ? "asc" : "desc")}");
            }

            return string.Join(",", orderByQuery);
        }
    }

    /// <summary>
    /// Gets or sets the collection ID. When set, selects only collections with this ID.
    /// </summary>
    public string CollectionId { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </summary>
    public bool AllDescendants { get; set; }

    public static FromQuery Create(string collectionId, bool allDescendants)
    {
        return new(collectionId, allDescendants);
    }

    internal FromQuery(string collectionId, bool allDescendants)
    {
        CollectionId = collectionId;
        AllDescendants = allDescendants;
    }
}
